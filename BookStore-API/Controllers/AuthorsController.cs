using AutoMapper;
using BookStore_API.Contracts;
using BookStore_API.Data;
using BookStore_API.Data.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BookStore_API.Controllers
{
    /// <summary>
    /// This Controller Intacts with Book Store Author Data Base
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public class AuthorsController : ControllerBase
    {
        private readonly IAuthorRepository _authorRepository;
        private readonly ILoggerService _logger;
        private readonly IMapper _mapper;

        public AuthorsController(IAuthorRepository authorRepository, ILoggerService logger, IMapper mapper)
        {
            _authorRepository = authorRepository;
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        /// For getting list of author
        /// </summary>
        /// <returns> List of author</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAuthors()
        {
            try
            {
                _logger.logInfo($"Attempted to get entire author list");
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                var authors = await _authorRepository.FindAll();
                var response = _mapper.Map<IList<AuthorDTO>>(authors);
                stopWatch.Stop();
                TimeSpan ts = stopWatch.Elapsed;

                // Format and display the TimeSpan value.
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                    ts.Hours, ts.Minutes, ts.Seconds,
                    ts.Milliseconds / 10);
                Console.WriteLine("RunTime " + elapsedTime);
                _logger.logInfo($"Successfully got  entire author list");
                return Ok(response);
            }
            catch (Exception e)
            {
                return Internalserver($"{e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// get author detail by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>author record data</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAuthor(int id)
        {
            try
            {
                _logger.logInfo($"Attempted to get author by id : {id}");
                var author = await _authorRepository.FindById(id);
                if (author == null)
                {
                    _logger.logWarning($"author id not found: {id}");
                    return NotFound();
                }
                var response = _mapper.Map<AuthorDTO>(author);
                _logger.logInfo($"Successfully got author by id: {id}");
                return Ok(response);
            }
            catch (Exception e)
            {
                return Internalserver($"{e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// For Creating new Author
        /// </summary>
        /// <param name="AuthorDTO"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] AuthorCreateDTO AuthorDTO)
        {
            try
            {
                _logger.logInfo($"Author submission attempted");
                if (AuthorDTO == null)
                {
                    _logger.logWarning($"Empty Request Submitted");
                    return BadRequest(ModelState);
                }
                if (!ModelState.IsValid)
                {
                    _logger.logWarning($"Author Data was Incomplete");
                    return BadRequest(ModelState);
                }
                var author = _mapper.Map<Author>(AuthorDTO);
                var isSuccessful = await _authorRepository.Create(author);
                if (!isSuccessful)
                {
                    return Internalserver($"Author creation Failed");
                }
                _logger.logInfo("Author Created Successfully");
                return Created("Created", new { author });
            }
            catch (Exception e)
            {
                return Internalserver($"{e.Message} - {e.InnerException}");
            }
        }


        /// <summary>
        /// Update author
        /// </summary>
        /// <param name="id"></param>
        /// <param name="AuthorDTO"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(int id, [FromBody] AuthorUpdateDTO AuthorDTO)
        {
            try
            {
               if(id < 1 || AuthorDTO == null || id != AuthorDTO.Id)
                {
                    _logger.logWarning($"Empty Request Submitted");
                    return BadRequest();
                }
                var isExist = await _authorRepository.isExists(id);
                if (!isExist)
                {
                    _logger.logWarning($" id : {id} does not exist");
                    return NotFound();
                }
                if (!ModelState.IsValid)
                {
                    _logger.logWarning($"Empty Request Submitted");
                    return BadRequest(ModelState);
                }
                var author = _mapper.Map<Author>(AuthorDTO);
                var isSuccessful = await _authorRepository.Update(author);
                if (!isSuccessful)
                {
                    return Internalserver($"Update Operation Failed");
                }
                _logger.logInfo("Author Updated Successfully");
                return NoContent();
            }
            catch (Exception e)
            {
                return Internalserver($"{e.Message} - {e.InnerException}");
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (id < 1)
                {
                    _logger.logWarning($"Empty Request Submitted");
                    return BadRequest();
                }
                if (!ModelState.IsValid)
                {
                    _logger.logWarning($"Empty Request Submitted");
                    return BadRequest(ModelState);
                }
                var author = await _authorRepository.FindById(id);
                if (author == null)
                {
                    _logger.logWarning($"author id not found: {id}");
                    return NotFound();
                }
                var isSuccessful = await _authorRepository.Delete(author);
                if (!isSuccessful)
                {
                    return Internalserver($"Delete Operation Failed");
                }
                _logger.logInfo("Delete Updated Successfully");
                return NoContent();
            }
            catch (Exception e)
            {
                return Internalserver($"{e.Message} - {e.InnerException}");
            }
        }

        private ObjectResult Internalserver(string message)
        {
            _logger.logError(message);
            return StatusCode(500, "Somthing went wrong contact your adminstrator");
        }
    }
}
