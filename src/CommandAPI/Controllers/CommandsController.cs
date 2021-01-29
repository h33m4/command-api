using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using CommandAPI.Data;
using CommandAPI.Models;
using AutoMapper;
using CommandAPI.Dtos;
using Microsoft.AspNetCore.JsonPatch;


namespace CommandAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommandsController: ControllerBase
    {
        private readonly ICommandAPIRepo _repository;
        private readonly IMapper _mapper;

        public CommandsController(ICommandAPIRepo repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        [HttpGet]
        public ActionResult<IEnumerable<CommandReadDto>> GetAllCommands() 
        {
            var commandItems = _repository.GetAllCommmands();

            return Ok(_mapper.Map<IEnumerable<CommandReadDto>>(commandItems));
        }

        [HttpGet("{id}", Name = "GetCommandById")]
        public ActionResult<CommandReadDto> GetCommandById(int id)
        {
            var commandItem = _repository.GetCommandById(id);

            if (commandItem == null) {
                return NotFound();
            }

            return Ok(_mapper.Map<CommandReadDto>(commandItem));
        }

        [HttpPost]
        public ActionResult<CommandReadDto> CreateCommand(CommandCreateDto commandCreateDto)
        {
            var commandModel = _mapper.Map<Command>(commandCreateDto);
            _repository.CreateCommand(commandModel);
            _repository.SaveChanges();

            var commandReadDto = _mapper.Map<CommandReadDto>(commandModel);

            return CreatedAtRoute(nameof(GetCommandById), new {Id = commandReadDto.Id}, commandReadDto);

        }

        [HttpPut("{id}")]
        public ActionResult UpdateCommand(CommandUpdateDto commandUpdateDto, int id)
        {
            var commandModelfromRepo = _repository.GetCommandById(id);
            if (commandModelfromRepo == null)
            {
                return NotFound();
            }

            _mapper.Map(commandUpdateDto, commandModelfromRepo);
            _repository.SaveChanges();

            return NoContent();
        }

        [HttpPatch("{id}")]
        public ActionResult PartialCommandUpdate(int id, JsonPatchDocument<CommandUpdateDto> patchDoc)
        {
            var CommandModelfromRepo = _repository.GetCommandById(id);
            if (CommandModelfromRepo == null)
            {
                return NotFound();
            }

            var commandToPatch = _mapper.Map<CommandUpdateDto>(CommandModelfromRepo);
            patchDoc.ApplyTo(commandToPatch, ModelState);

            if (!TryValidateModel(commandToPatch)) 
            {
                return ValidationProblem(ModelState);
            }

            _mapper.Map(commandToPatch, CommandModelfromRepo);
            _repository.UpdateCommand(CommandModelfromRepo);
            _repository.SaveChanges();

            return NoContent();
        }

        [HttpDelete("{id")]
        public ActionResult DeleteCommand(int id)
        {
            var CommandToBeRemoved = _repository.GetCommandById(id);

            if (CommandToBeRemoved == null)
            {
                return NotFound();
            }

            _repository.DeleteCommand(CommandToBeRemoved);
            _repository.SaveChanges();
            
            return NoContent();
        }
    }
}