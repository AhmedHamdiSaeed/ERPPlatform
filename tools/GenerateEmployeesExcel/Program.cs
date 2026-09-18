using System;
using System.IO;
using System.Text.RegularExpressions;
using NPOI.SS.UserModel;
using NPOI.XSSF.Streaming;
using NPOI.XSSF.UserModel;

namespace GenerateEmployeesExcel;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Generating 10,000 realistic employees Excel workbook...");

        var maleFirstNames = new[]
        {
            "Ahmed", "Mohamed", "Mahmoud", "Omar", "Youssef", "Karim", "Tarek", "Hany", "Sherif", "Mostafa",
            "Amr", "Hossam", "Khaled", "Walid", "Rami", "Fady", "Mina", "George", "Peter", "Sameh",
            "Marwan", "Ziad", "Hazem", "Nader", "Bassem", "Wael", "Essam", "Ehab", "Magdy", "Ashraf",
            "Medhat", "Aly", "Hatem", "Sameer", "Hesham", "Nabil", "Ayman", "Emad", "Raouf", "Bahaa",
            "Adam", "David", "Michael", "James", "John", "Robert", "Daniel", "Thomas", "Alexander", "Lucas"
        };

        var femaleFirstNames = new[]
        {
            "Sarah", "Mona", "Aya", "Rania", "Salma", "Mariam", "Dina", "Nour", "Layla", "Yasmin",
            "Hagar", "Reem", "Heba", "Nihal", "Radwa", "Dalia", "Menna", "May", "Omnia", "Esraa",
            "Doaa", "Shaimaa", "Noha", "Samah", "Ghada", "Samar", "Lobna", "Donia", "Zeinab", "Fatma",
            "Hana", "Judy", "Malak", "Karma", "Farida", "Shahd", "Nada", "Ingy", "Sandra", "Christine",
            "Mary", "Jessica", "Emily", "Laura", "Olivia", "Emma", "Sophia", "Chloe", "Grace", "Rachel"
        };

        var lastNames = new[]
        {
            "El-Sayed", "Hassan", "Mahmoud", "Ibrahim", "Ali", "Khalil", "Mostafa", "Abdelrahman", "Farouk", "Mansour",
            "Soliman", "Osman", "Radwan", "Fawzy", "Nabil", "Kamel", "Gamal", "Hamed", "Shaker", "Metwally",
            "Sallam", "Zaki", "Hegazy", "Darwish", "Attia", "Badawy", "Helmy", "Ezzat", "Gaber", "Ghoneim",
            "Ashour", "Bayoumi", "Kassem", "Wahba", "Sobhy", "Barakat", "Tawfik", "Qasim", "Allam", "El-Gohary",
            "Smith", "Johnson", "Williams", "Brown", "Jones", "Miller", "Davis", "Wilson", "Taylor", "Clark"
        };

        var departments = new[]
        {
            "Engineering", "Human Resources", "Finance", "Sales", "Logistics",
            "Information Technology", "Marketing", "Customer Support", "Operations", "Legal & Compliance"
        };

        var deptPositions = new System.Collections.Generic.Dictionary<string, string[]>
        {
            ["Engineering"] = new[] { "Senior Software Engineer", "Full Stack Developer", "Backend .NET Architect", "Frontend Angular Specialist", "DevOps & Cloud Engineer", "QA Automation Lead", "Engineering Lead" },
            ["Human Resources"] = new[] { "HR Business Partner", "Talent Acquisition Specialist", "Payroll & Benefits Officer", "HR Generalist", "People Operations Lead" },
            ["Finance"] = new[] { "Senior Financial Analyst", "General Ledger Accountant", "Accounts Payable Specialist", "Budgeting Specialist", "Senior Auditor" },
            ["Sales"] = new[] { "Enterprise Sales Director", "Senior Account Executive", "Business Development Manager", "Sales Representative", "Key Account Specialist" },
            ["Logistics"] = new[] { "Supply Chain Coordinator", "Warehouse Supervisor", "Inventory Control Specialist", "Logistics Operations Lead", "Fleet Coordinator" },
            ["Information Technology"] = new[] { "System Administrator", "Network Security Engineer", "Database Administrator", "IT Support Specialist", "Cloud Infrastructure Engineer" },
            ["Marketing"] = new[] { "Digital Marketing Strategist", "Content & Brand Manager", "SEO Specialist", "Social Media Coordinator", "Growth Marketing Lead" },
            ["Customer Support"] = new[] { "Customer Success Lead", "Technical Support Engineer", "Client Relations Specialist", "Helpdesk Supervisor" },
            ["Operations"] = new[] { "Operations Manager", "Process Optimization Analyst", "Facility Coordinator", "Operations Specialist" },
            ["Legal & Compliance"] = new[] { "Legal Counsel", "Compliance Auditor", "Contracts Specialist", "Risk Assessment Officer" }
        };

        var locations = new[] { "Cairo HQ", "Alexandria Branch", "Giza Tech Hub", "Smart Village Office", "Mansoura Center", "Dubai Regional Office" };
        var managers = new[] { "Eng. Ahmed Hamdi", "Dr. Tarek Mansour", "Sarah Abdelrahman", "Mohamed El-Sayed", "Mona Khalil", "Karim Farouk", "Sherif Kamel" };

        var rand = new Random(42);

        // Use SXSSFWorkbook for memory efficiency
        using var xssfWorkbook = new XSSFWorkbook();
        using var workbook = new SXSSFWorkbook(xssfWorkbook, 500);
        var sheet = workbook.CreateSheet("Employees");

        // Header style
        var headerFont = workbook.CreateFont();
        headerFont.IsBold = true;
        headerFont.FontHeightInPoints = 11;
        headerFont.Color = IndexedColors.White.Index;

        var headerStyle = workbook.CreateCellStyle();
        headerStyle.SetFont(headerFont);
        headerStyle.FillForegroundColor = IndexedColors.RoyalBlue.Index;
        headerStyle.FillPattern = FillPattern.SolidForeground;
        headerStyle.Alignment = HorizontalAlignment.Center;
        headerStyle.VerticalAlignment = VerticalAlignment.Center;

        var headers = new[]
        {
            "EmployeeCode", "Name", "Email", "Phone", "Position",
            "DepartmentName", "Salary", "JoiningDate", "Status",
            "Location", "ManagerName", "LeaveBalance"
        };

        var headerRow = sheet.CreateRow(0);
        headerRow.HeightInPoints = 26;
        for (int c = 0; c < headers.Length; c++)
        {
            var cell = headerRow.CreateCell(c);
            cell.SetCellValue(headers[c]);
            cell.CellStyle = headerStyle;
        }

        var usedEmails = new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (int i = 1; i <= 10000; i++)
        {
            var isFemale = rand.NextDouble() < 0.45;
            var firstName = isFemale
                ? femaleFirstNames[rand.Next(femaleFirstNames.Length)]
                : maleFirstNames[rand.Next(maleFirstNames.Length)];
            var lastName = lastNames[rand.Next(lastNames.Length)];
            var fullName = $"{firstName} {lastName}";

            // Generate clean unique email
            var cleanFirst = Regex.Replace(firstName.ToLowerInvariant(), "[^a-z]", "");
            var cleanLast = Regex.Replace(lastName.ToLowerInvariant(), "[^a-z]", "");
            var emailBase = $"{cleanFirst}.{cleanLast}";
            var email = $"{emailBase}@erpplatform.com";
            if (usedEmails.Contains(email))
            {
                email = $"{emailBase}.{i}@erpplatform.com";
            }
            usedEmails.Add(email);

            var phonePrefixes = new[] { "+2010", "+2011", "+2012", "+2015" };
            var prefix = phonePrefixes[rand.Next(phonePrefixes.Length)];
            var phone = $"{prefix}{rand.Next(10000000, 99999999)}";

            var dept = departments[rand.Next(departments.Length)];
            var posList = deptPositions[dept];
            var position = posList[rand.Next(posList.Length)];

            // Salary between 15000 and 85000 in steps of 500
            var salary = rand.Next(30, 170) * 500;

            // Joining Date between 2021-01-01 and 2026-08-30
            var start = new DateTime(2021, 1, 1);
            var daysRange = (new DateTime(2026, 8, 30) - start).Days;
            var joinDate = start.AddDays(rand.Next(daysRange)).ToString("yyyy-MM-dd");

            // Status: mostly Active
            var statusRoll = rand.NextDouble();
            var status = statusRoll < 0.94 ? "Active" : (statusRoll < 0.98 ? "On Leave" : "Inactive");

            var location = locations[rand.Next(locations.Length)];
            var manager = managers[rand.Next(managers.Length)];
            var leaveBalance = rand.Next(14, 31);

            var code = $"EMP-{i:D5}";

            var row = sheet.CreateRow(i);
            row.CreateCell(0).SetCellValue(code);
            row.CreateCell(1).SetCellValue(fullName);
            row.CreateCell(2).SetCellValue(email);
            row.CreateCell(3).SetCellValue(phone);
            row.CreateCell(4).SetCellValue(position);
            row.CreateCell(5).SetCellValue(dept);
            row.CreateCell(6).SetCellValue(salary);
            row.CreateCell(7).SetCellValue(joinDate);
            row.CreateCell(8).SetCellValue(status);
            row.CreateCell(9).SetCellValue(location);
            row.CreateCell(10).SetCellValue(manager);
            row.CreateCell(11).SetCellValue(leaveBalance);

            if (i % 2000 == 0)
            {
                Console.WriteLine($"Generated {i:N0} rows...");
            }
        }

        var outputPath = Path.Combine(Directory.GetCurrentDirectory(), "10000_Employees_ERP_Import.xlsx");
        using (var fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
        {
            workbook.Write(fs);
        }

        Console.WriteLine($"Successfully generated 10,000 employees workbook at: {outputPath}");
        Console.WriteLine($"File size: {new FileInfo(outputPath).Length / 1024.0:F1} KB");
    }
}
