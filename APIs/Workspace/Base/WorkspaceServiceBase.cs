using Microsoft.EntityFrameworkCore;
using MyService.APIs.Dtos;
using MyService.APIs.Errors;
using MyService.APIs.Extensions;
using MyService.Infrastructure;
using MyService.Infrastructure.Models;
using NuGet.Packaging;

namespace MyService.APIs;

public abstract class WorkspacesServiceBase : IWorkspacesService
{
    protected readonly MyServiceContext _context;

    public WorkspacesServiceBase(MyServiceContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<WorkspaceDto>> Workspaces(WorkspaceFindMany findManyArgs)
    {
        var workspaces = await _context
            .Workspaces.Include(x => x.TodoItems)
            .ApplyWhere(findManyArgs.Where)
            .ApplySkip(findManyArgs.Skip)
            .ApplyTake(findManyArgs.Take)
            .ApplyOrderBy(findManyArgs.SortBy)
            .ToListAsync();

        return workspaces.ConvertAll(workspace => workspace.ToDto());
    }

    public async Task<WorkspaceDto> Workspace(WorkspaceIdDto idDto)
    {
        var workspace = await _context.Workspaces.FindAsync(idDto.Id);

        if (workspace == null)
        {
            throw new NotFoundException();
        }

        return workspace.ToDto();
    }

    public async Task UpdateWorkspace(WorkspaceIdDto idDto, WorkspaceUpdateInput updateDto)
    {
        var workspace = updateDto.ToModel(idDto);

        if (updateDto.TodoItemIds != null)
        {
            workspace.TodoItems = await _context
                .TodoItems.Where(todo => updateDto.TodoItemIds.Select(t => t.Id).Contains(todo.Id))
                .ToListAsync();
        }

        _context.Entry(workspace).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!WorkspaceExists(idDto))
            {
                throw new NotFoundException();
            }
            else
            {
                throw;
            }
        }
    }

    public async Task<WorkspaceDto> CreateWorkspace(WorkspaceCreateInput createDto)
    {
        var model = new Workspace { Name = createDto.Name, };
        if (createDto.Id != null)
        {
            model.Id = createDto.Id.Value;
        }

        if (createDto.TodoItemIds != null)
        {
            model.TodoItems = await _context
                .TodoItems.Where(todo => createDto.TodoItemIds.Select(t => t.Id).Contains(todo.Id))
                .ToListAsync();
        }

        _context.Workspaces.Add(model);
        await _context.SaveChangesAsync();

        var result = await _context.FindAsync<Workspace>(model.Id);

        if (result == null)
        {
            throw new NotFoundException();
        }

        return result.ToDto();
    }

    public async Task DeleteWorkspace(WorkspaceIdDto idDto)
    {
        var workspace = await _context.Workspaces.FindAsync(idDto.Id);
        if (workspace == null)
        {
            throw new NotFoundException();
        }

        _context.Workspaces.Remove(workspace);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<TodoItemDto>> TodoItems(
        WorkspaceIdDto idDto,
        TodoItemFindMany todoItemFindMany
    )
    {
        var workspace = await _context.Workspaces.FirstAsync(x => x.Id == idDto.Id);

        if (workspace == null)
        {
            throw new NotFoundException();
        }

        return workspace.TodoItems.Select(todo => todo.ToDto()).ToList();
    }

    public async Task ConnectTodoItems(WorkspaceIdDto idDto, TodoItemIdDto[] todoItemsId)
    {
        var workspace = await _context
            .Workspaces.Include(x => x.TodoItems)
            .FirstOrDefaultAsync(x => x.Id == idDto.Id);
        if (workspace == null)
        {
            throw new NotFoundException();
        }

        var todoItems = await _context
            .TodoItems.Where(t => todoItemsId.Select(x => x.Id).Contains(t.Id))
            .ToListAsync();
        if (todoItems.Count == 0)
        {
            throw new NotFoundException();
        }

        var newTodoItems = todoItems.Except(workspace.TodoItems);
        workspace.TodoItems.AddRange(newTodoItems);
        await _context.SaveChangesAsync();
    }

    public async Task DisconnectTodoItems(WorkspaceIdDto idDto, TodoItemIdDto[] todoItemsId)
    {
        var workspace = await _context
            .Workspaces.Include(x => x.TodoItems)
            .FirstOrDefaultAsync(x => x.Id == idDto.Id);

        if (workspace == null)
        {
            throw new NotFoundException();
        }

        var todoItems = await _context
            .TodoItems.Where(t => todoItemsId.Select(x => x.Id).Contains(t.Id))
            .ToListAsync();

        foreach (var todoItem in todoItems)
        {
            workspace.TodoItems.Remove(todoItem);
        }
        await _context.SaveChangesAsync();
    }

    private bool WorkspaceExists(WorkspaceIdDto idDto)
    {
        return _context.Workspaces.Any(e => e.Id == idDto.Id);
    }
}
