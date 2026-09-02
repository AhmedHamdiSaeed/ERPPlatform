using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using ERPPlatform.Documents;

namespace ERPPlatform.Application.Documents;

public class FolderDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid? ParentId { get; set; }
}

public class FolderAppService : ApplicationService
{
    private readonly IRepository<Folder, Guid> _folderRepository;

    public FolderAppService(IRepository<Folder, Guid> folderRepository)
    {
        _folderRepository = folderRepository;
    }

    public async Task<List<FolderDto>> GetListAsync(Guid? parentId)
    {
        var folders = await _folderRepository.GetListAsync(f => f.ParentId == parentId);
        return folders.Select(f => new FolderDto
        {
            Id = f.Id,
            Name = f.Name,
            ParentId = f.ParentId
        }).ToList();
    }

    public async Task<FolderDto> CreateAsync(string name, Guid? parentId)
    {
        var folder = new Folder { Name = name, ParentId = parentId };
        await _folderRepository.InsertAsync(folder);
        return new FolderDto { Id = folder.Id, Name = folder.Name, ParentId = folder.ParentId };
    }

    public async Task DeleteAsync(Guid id)
    {
        await _folderRepository.DeleteAsync(id);
    }
}
