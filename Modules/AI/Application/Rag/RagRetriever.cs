using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ERPPlatform.Documents;
using ERPPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace ERPPlatform.Modules.AI.Application.Rag;

/// <summary>
/// A single retrieved record, rendered as compact searchable text.
/// </summary>
public class RagChunk
{
    public string Source { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public int Score { get; set; }
}

/// <summary>
/// Retrieves records from the live ERP database that are relevant to a
/// natural-language question, so the LLM can answer with real data (RAG).
/// Uses lexical (keyword) matching — no external vector store required.
/// </summary>
public interface IRagRetriever
{
    Task<string> RetrieveContextAsync(string query, int maxChunks = 8);
}

public class RagRetriever : IRagRetriever
{
    private readonly IRepository<Product, Guid> _products;
    private readonly IRepository<Employee, Guid> _employees;
    private readonly IRepository<SalesInvoice, Guid> _invoices;
    private readonly IRepository<Customer, Guid> _customers;
    private readonly IRepository<Supplier, Guid> _suppliers;
    private readonly IRepository<Document, Guid> _documents;

    public RagRetriever(
        IRepository<Product, Guid> products,
        IRepository<Employee, Guid> employees,
        IRepository<SalesInvoice, Guid> invoices,
        IRepository<Customer, Guid> customers,
        IRepository<Supplier, Guid> suppliers,
        IRepository<Document, Guid> documents)
    {
        _products = products;
        _employees = employees;
        _invoices = invoices;
        _customers = customers;
        _suppliers = suppliers;
        _documents = documents;
    }

    public async Task<string> RetrieveContextAsync(string query, int maxChunks = 8)
    {
        var keywords = Tokenize(query);
        if (keywords.Count == 0)
        {
            return string.Empty;
        }

        var chunks = new List<RagChunk>();

        await CollectAsync(
            _products, keywords,
            new Expression<Func<Product, string>>[] { p => p.Name, p => p.Sku, p => p.Barcode, p => p.Category, p => p.WarehouseName, p => p.SupplierName, p => p.Status },
            p => $"Product {p.Name} (SKU {p.Sku}, category {p.Category}), stock {p.Stock} {p.Unit} at price {p.Price}, warehouse {p.WarehouseName}, status {p.Status}, supplier {p.SupplierName}.",
            p => Score(keywords, p.Name, p.Sku, p.Category, p.WarehouseName, p.SupplierName, p.Status),
            "Product", chunks);

        await CollectAsync(
            _employees, keywords,
            new Expression<Func<Employee, string>>[] { e => e.Name, e => e.EmployeeCode, e => e.Email, e => e.DepartmentName, e => e.Position, e => e.Location, e => e.Status },
            e => $"Employee {e.Name} ({e.EmployeeCode}), {e.Position} in {e.DepartmentName}, location {e.Location}, status {e.Status}, leave balance {e.LeaveBalance} days.",
            e => Score(keywords, e.Name, e.EmployeeCode, e.DepartmentName, e.Position, e.Location, e.Status),
            "Employee", chunks);

        await CollectAsync(
            _invoices, keywords,
            new Expression<Func<SalesInvoice, string>>[] { i => i.InvoiceNumber, i => i.CustomerName, i => i.CustomerEmail, i => i.Status, i => i.Notes, i => i.CreatedBy },
            i => $"Sales invoice {i.InvoiceNumber} for {i.CustomerName}, total {i.TotalAmount}, status {i.Status}, issued {i.IssueDate:yyyy-MM-dd}, due {i.DueDate:yyyy-MM-dd}.",
            i => Score(keywords, i.InvoiceNumber, i.CustomerName, i.Status, i.CreatedBy),
            "SalesInvoice", chunks);

        await CollectAsync(
            _customers, keywords,
            new Expression<Func<Customer, string>>[] { c => c.Name, c => c.CustomerCode, c => c.Email, c => c.ContactPerson, c => c.TaxNumber, c => c.Currency },
            c => $"Customer {c.Name} ({c.CustomerCode}), contact {c.ContactPerson}, credit limit {c.CreditLimit} {c.Currency}, outstanding {c.OutstandingBalance}, terms {c.PaymentTerms}.",
            c => Score(keywords, c.Name, c.CustomerCode, c.ContactPerson, c.TaxNumber, c.Currency),
            "Customer", chunks);

        await CollectAsync(
            _suppliers, keywords,
            new Expression<Func<Supplier, string>>[] { s => s.CompanyName, s => s.SupplierCode, s => s.Email, s => s.ContactPerson, s => s.TaxNumber },
            s => $"Supplier {s.CompanyName} ({s.SupplierCode}), contact {s.ContactPerson}, outstanding {s.OutstandingBalance}, terms {s.PaymentTerms}.",
            s => Score(keywords, s.CompanyName, s.SupplierCode, s.ContactPerson, s.TaxNumber),
            "Supplier", chunks);

        await CollectAsync(
            _documents, keywords,
            new Expression<Func<Document, string>>[] { d => d.Title, d => d.Extension, d => d.ContentType },
            d => $"Document {d.Title} ({d.Extension}, {d.ContentType}, {d.SizeBytes} bytes).",
            d => Score(keywords, d.Title, d.Extension, d.ContentType),
            "Document", chunks);

        var top = chunks
            .OrderByDescending(c => c.Score)
            .ThenBy(c => c.Source)
            .Take(maxChunks)
            .ToList();

        if (top.Count == 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        sb.AppendLine("Relevant ERP records (use these to answer):");
        foreach (var c in top)
        {
            sb.AppendLine($"- [{c.Source}] {c.Text}");
        }
        return sb.ToString();
    }

    private async Task CollectAsync<T>(
        IRepository<T, Guid> repository,
        List<string> keywords,
        Expression<Func<T, string>>[] fieldSelectors,
        Func<T, string> render,
        Func<T, int> score,
        string source,
        List<RagChunk> output) where T : class, IEntity<Guid>
    {
        var predicate = BuildPredicate(keywords, fieldSelectors);
        var matches = await (await repository.GetQueryableAsync())
            .Where(predicate)
            .Take(5)
            .ToListAsync();

        foreach (var m in matches)
        {
            output.Add(new RagChunk
            {
                Source = source,
                Text = render(m),
                Score = score(m)
            });
        }
    }

    private static Expression<Func<T, bool>> BuildPredicate<T>(
        List<string> keywords,
        Expression<Func<T, string>>[] fieldSelectors) where T : class
    {
        var param = Expression.Parameter(typeof(T), "e");
        Expression? body = null;

        foreach (var selector in fieldSelectors)
        {
            var member = new ParameterRebinder(selector.Parameters[0], param).Visit(selector.Body)!;
            foreach (var kw in keywords)
            {
                var containsCall = Expression.Call(
                    member,
                    typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) })!,
                    Expression.Constant(kw));

                body = body == null
                    ? (Expression)containsCall
                    : Expression.OrElse(body, containsCall);
            }
        }

        return Expression.Lambda<Func<T, bool>>(body ?? Expression.Constant(false), param);
    }

    private static int Score(List<string> keywords, params string?[] fields)
    {
        var score = 0;
        foreach (var f in fields)
        {
            if (string.IsNullOrEmpty(f))
            {
                continue;
            }
            var lower = f.ToLowerInvariant();
            score += keywords.Count(k => lower.Contains(k));
        }
        return score;
    }

    private static readonly HashSet<string> Stopwords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "the", "a", "an", "of", "for", "to", "in", "on", "at", "is", "are", "was", "were", "be",
        "and", "or", "with", "what", "how", "who", "when", "where", "why", "which", "show", "me",
        "list", "all", "my", "our", "please", "can", "you", "do", "does", "did", "has", "have",
        "had", "get", "give", "find", "tell", "about", "that", "this", "these", "those", "from",
        "by", "as", "it", "its", "we", "i", "they", "he", "she", "any", "some", "there", "their",
        "his", "her", "your", "would", "could", "should", "will", "us", "than", "then", "into"
    };

    private static List<string> Tokenize(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return new List<string>();
        }

        return query
            .ToLowerInvariant()
            .Split(new[] { ' ', '\t', '\n', '\r', '.', ',', ';', ':', '?', '!', '"', '\'', '(', ')', '-', '_', '/', '\\', '@', '#' },
                StringSplitOptions.RemoveEmptyEntries)
            .Where(t => t.Length >= 2 && !Stopwords.Contains(t))
            .Distinct()
            .Take(8)
            .ToList();
    }

    private class ParameterRebinder : ExpressionVisitor
    {
        private readonly ParameterExpression _old;
        private readonly ParameterExpression _new;

        public ParameterRebinder(ParameterExpression oldParam, ParameterExpression newParam)
        {
            _old = oldParam;
            _new = newParam;
        }

        protected override Expression VisitParameter(ParameterExpression node)
            => node == _old ? _new : base.VisitParameter(node);
    }
}
