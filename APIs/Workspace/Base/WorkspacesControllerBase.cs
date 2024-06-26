using Microsoft.AspNetCore.Mvc;
using MyService.APIs.Dtos;
using MyService.APIs.Errors;

namespace MyService.APIs;

[Route("api/[controller]")]
[ApiController]
public abstract class WorkspacesControllerBase : ControllerBase
{
    protected readonly IWorkspacesService _service;

    public WorkspacesControllerBase(IWorkspacesService service)
    {
        _service = service;
    }

    // GET: api/author
    [HttpGet]
    public async Task<ActionResult<IEnumerable<WorkspaceDto>>> Workspaces(
        [FromQuery] WorkspaceFindMany filter
    )
    {
        return Ok(await _service.Workspaces(filter));
    }

    // GET: api/author/5
    [HttpGet("{Id}")]
    public async Task<ActionResult<WorkspaceDto>> Workspace([FromRoute] WorkspaceIdDto idDto)
    {
        try
        {
            return await _service.Workspace(idDto);
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }

    // PATCH: api/author/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPatch("{Id}")]
    public async Task<IActionResult> UpdateWorkspace(
        [FromRoute] WorkspaceIdDto idDto,
        [FromQuery] WorkspaceUpdateInput authorUpdateDto
    )
    {
        try
        {
            await _service.UpdateWorkspace(idDto, authorUpdateDto);
        }
        catch (NotFoundException)
        {
            return NotFound();
        }

        return NoContent();
    }

    // POST: api/author
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<WorkspaceDto>> CreateWorkspace(WorkspaceCreateInput input)
    {
        var author = await _service.CreateWorkspace(input);

        return CreatedAtAction(nameof(Workspace), new { id = author.Id }, author);
    }

    // DELETE: api/author/5
    [HttpDelete("{Id}")]
    public async Task<IActionResult> DeleteTodoItem([FromRoute] WorkspaceIdDto idDto)
    {
        try
        {
            await _service.DeleteWorkspace(idDto);
        }
        catch (NotFoundException)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Get all TodoItems of an Workspace
    /// </summary>
    /// <returns></returns>
    [HttpGet("{Id}/todoItems")]
    public async Task<IActionResult> TodoItems(
        [FromRoute] WorkspaceIdDto idDto,
        [FromQuery] TodoItemFindMany filter
    )
    {
        try
        {
            return Ok(await _service.TodoItems(idDto, filter));
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Connect a TodoItem to an Workspace
    /// </summary>
    [HttpPost("{Id}/todoItems")]
    public async Task<IActionResult> ConnectTodoItem(
        [FromRoute] WorkspaceIdDto idDto,
        [FromBody] TodoItemIdDto[] todoItemIds
    )
    {
        try
        {
            await _service.ConnectTodoItems(idDto, todoItemIds);
        }
        catch (NotFoundException)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Disconnect a TodoItem from an Workspace
    /// </summary>
    [HttpDelete("{Id}/todoItems")]
    public async Task<IActionResult> DisconnectTodoItem(
        [FromRoute] WorkspaceIdDto idDto,
        [FromBody] TodoItemIdDto[] todoItemIds
    )
    {
        try
        {
            await _service.DisconnectTodoItems(idDto, todoItemIds);
        }
        catch (NotFoundException)
        {
            return NotFound();
        }

        return NoContent();
    }
}
