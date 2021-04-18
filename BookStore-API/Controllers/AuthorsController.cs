using AutoMapper;
using BookStore_API.Contracts;
using BookStore_API.Data;
using BookStore_API.Data.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public AuthorsController( IAuthorRepository authorRepository, ILoggerService logger, IMapper mapper)
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
        public async Task<IActionResult> GetAuthors() {
            try
            {
                _logger.logInfo($"Attempted to get entire author list");
                var authors = await _authorRepository.FindAll();
                var response = _mapper.Map<IList<AuthorDTO>>(authors);
                _logger.logInfo($"Successfully got  entire author list");
                return Ok(response);
            }
            catch(Exception e)
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
                var response = _mapper.Map<IList<AuthorDTO>>(author);
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
        public async Task<IActionResult> Create([FromBody] AuthorCreateDTO AuthorDTO )
        {
            try
            {
                _logger.logInfo($"Author submission attempted");
               if(AuthorDTO == null)
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

        private ObjectResult Internalserver(string message)
        {
            _logger.logError(message);
            return StatusCode(500, "Somthing went wrong contact your adminstrator");
        }
    }
}
