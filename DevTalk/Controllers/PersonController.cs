// What is a (pure) REST API?
// 1. Maps HTTP methods to CRUD operations
// 2. Maps HTTP response codes to results
// 3. Does not have to be JSON

// Why isn't your REST API really a REST API?
// 1. Not organized by entity: /api/v1/{entity}/{id?}
// 2. Not leveraging HTTP methods for CRUD operations
// 3. Not using HTTP status codes for success/failure indications
// 4. Your endpoints feel like RPC

// Why does this often happen?

// What does this look like in C# (.NET Core 3.1 MVC)

using System;
using System.Collections.Generic;
using System.Linq;
using DevTalk.Models;
using Microsoft.AspNetCore.Mvc;

namespace DevTalk.Controllers
{
    // NOTE: The data storage implementation is not even close to thread-safe. It is purely
    // intended as a way to show the endpoints with some form of actual data going across
    // the API, not as an example of how you might implement the logic for each endpoint

    [Route("/api/v1/[Controller]")]
    public class PersonController : Controller
    {
        [HttpGet]
        public IActionResult List(int skip = 0, int take = 50)
        {
            // Ensure skip is valid
            skip = Math.Max(0, skip);

            return Ok(_data.Skip(skip).Take(take).ToList());
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            Person record = _data.FirstOrDefault(p => p.Id == id);
            return record != null ? (IActionResult)Ok(record) : NotFound();
            /* The line above is the same as:
            if (record != null)
                return Ok(record)
            else
                return NotFound();
            */
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] Person record)
        {
            // ID specified in URL overrides any in the request body
            record.Id = id;
            _data.Add(record);
            return Created($"/api/v1/person/{id}", record);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            Person record = _data.FirstOrDefault(p => p.Id == id);

            // Implementation choice: we've decided that attempts to delete non-existent records should fail
            if (record is null)
                return NotFound();

            _data.Remove(record);
            return NoContent(); // This is HTTP 204: https://http.cat/204
        }

        [HttpPatch("{id}")]
        public IActionResult Patch(int id, [FromBody] Person record)
        {
            // NOTE: Object merging is actually pretty tricky, so for now we do a full replace of the object

            // ID specified in URL overrides any in the request body
            record.Id = id;

            // Remove the old record
            Person oldRecord = _data.FirstOrDefault(p => p.Id == id);

            // Implementation choice: we've decided that attempts to update non-existent records should fail
            if (oldRecord is null)
                return NotFound();

            _data.Remove(oldRecord);

            // And add the new record
            _data.Add(record);

            return Ok(record);
        }

        [HttpPost]
        public IActionResult Post([FromBody] Person record)
        {
            // If the ID isn't set or refers to a nonexistent record
            if (record.Id > 0 || !_data.Any(p => p.Id == record.Id))
            {
                // Treat as create
                record.Id = _data.Max(p => p.Id) + 1;
                _data.Add(record);
            }
            else
            {
                // Treat as update
                Person oldRecord = _data.FirstOrDefault(p => p.Id == record.Id);
                _data.Remove(oldRecord);
                _data.Remove(record);
            }
            return Ok(record);
        }

        private static IList<Person> _data = new List<Person>
        {
            new Person
            {
                Id = 1,
                FirstName = "John",
                LastName = "Smith",
                Age = 34
            },
            new Person
            {
                Id = 2,
                FirstName = "Jane",
                LastName = "Doe",
                Age = 23
            },
            new Person
            {
                Id = 3,
                FirstName = "Bob",
                LastName = "Robertson"
            }
        };
    }
}