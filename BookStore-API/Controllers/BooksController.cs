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
using System.Threading.Tasks;

namespace BookStore_API.Controllers
{   /// <summary>
    /// Book table contact
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly IBookRepository _bookRepository;
        private readonly ILoggerService _logger;
        private readonly IMapper _mapper;

        public BooksController(IBookRepository bookRepository, ILoggerService logger, IMapper mapper)
        {
            _bookRepository = bookRepository;
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
        public async Task<IActionResult> GetBooks()
        {
            var getLocation = GetControllerActionNames();
            try
            {
                _logger.logInfo($"{getLocation} Attempted to get entire Book list");
                var books = await _bookRepository.FindAll();
                var response = _mapper.Map<IList<BookDTO>>(books);
                _logger.logInfo($"{getLocation} Successfully got  entire Book list");
                return Ok(response);
            }
            catch (Exception e)
            {
                return Internalserver($"{getLocation} - {e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// get book detail by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>book record data</returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetBook(int id)
        {
            var getLocation = GetControllerActionNames();
            try
            {
                _logger.logInfo($"{getLocation} Attempted to get book by id : {id}");
                var book = await _bookRepository.FindById(id);
                if (book == null)
                {
                    _logger.logWarning($"book id not found: {id}");
                    return NotFound();
                }
                var response = _mapper.Map<BookDTO>(book);
                _logger.logInfo($"{getLocation} Successfully got book by id: {id}");
                return Ok(response);
            }
            catch (Exception e)
            {
                return Internalserver($"{e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// For Creating new Book
        /// </summary>
        /// <param name="bookDTO"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] BookCreateDTO bookDTO)
        {
            var getLocation = GetControllerActionNames();
            try
            {
                _logger.logInfo($"{getLocation} book submission attempted");
                if (bookDTO == null)
                {
                    _logger.logWarning($"Empty Request Submitted");
                    return BadRequest(ModelState);
                }
                if (!ModelState.IsValid)
                {
                    _logger.logWarning($"book Data was Incomplete");
                    return BadRequest(ModelState);
                }
                var book = _mapper.Map<Book>(bookDTO);
                var isSuccessful = await _bookRepository.Create(book);
                if (!isSuccessful)
                {
                    return Internalserver($"book creation Failed");
                }
                _logger.logInfo($" {getLocation} book Created Successfully");
                return Created("Created", new { book });
            }
            catch (Exception e)
            {
                return Internalserver($"{e.Message} - {e.InnerException}");
            }
        }

        /// <summary>
        /// Update book
        /// </summary>
        /// <param name="id"></param>
        /// <param name="BookDTO"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(int id, [FromBody] BookUpdateDTO BookDTO)
        {
            var getLocation = GetControllerActionNames();
            try
            {
                _logger.logWarning($"{getLocation} Request Submitted Update");
                if (id < 1 || BookDTO == null || id != BookDTO.Id)
                {
                    _logger.logWarning($"Empty Request Submitted");
                    return BadRequest();
                }
                var isExist = await _bookRepository.isExists(id);
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
                var book = _mapper.Map<Book>(BookDTO);
                var isSuccessful = await _bookRepository.Update(book);
                if (!isSuccessful)
                {
                    return Internalserver($"Update Operation Failed");
                }
                _logger.logInfo($" {getLocation} book Updated Successfully");
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
            var getLocation = GetControllerActionNames();
            _logger.logWarning($"{getLocation} Started Book Delete");
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
                var book = await _bookRepository.FindById(id);
                if (book == null)
                {
                    _logger.logWarning($"book id not found: {id}");
                    return NotFound();
                }
                var isSuccessful = await _bookRepository.Delete(book);
                if (!isSuccessful)
                {
                    return Internalserver($"Delete Operation Failed");
                }
                _logger.logInfo($"{getLocation} Delete Updated Successfully");
                return NoContent();
            }
            catch (Exception e)
            {
                return Internalserver($"{e.Message} - {e.InnerException}");
            }
        }

        private string GetControllerActionNames()
        {
            var controller = ControllerContext.ActionDescriptor.ControllerName;
            var action = ControllerContext.ActionDescriptor.ActionName;

            return $"{controller} - {action}";

        }

        private ObjectResult Internalserver(string message)
        {
            _logger.logError(message);
            return StatusCode(500, "Somthing went wrong contact your adminstrator");
        }
    }
}
