﻿using Grand.Api.DTOs.Catalog;
using Grand.Api.Services;
using Grand.Services.Security;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;

namespace Grand.Web.Areas.Api.Controllers.OData
{
    public partial class CategoryController : BaseODataController
    {
        private readonly ICategoryApiService _categoryApiService;
        private readonly IPermissionService _permissionService;
        public CategoryController(ICategoryApiService categoryApiService, IPermissionService permissionService)
        {
            _categoryApiService = categoryApiService;
            _permissionService = permissionService;
        }

        [HttpGet]
        public IActionResult Get(string key)
        {
            if (!_permissionService.Authorize(PermissionSystemName.Categories))
                return Forbid();

            var category = _categoryApiService.GetById(key);
            if (category == null)
                return NotFound();

            return Ok(category);
        }

        [HttpGet]
        [EnableQuery]
        public IActionResult Get()
        {
            if (!_permissionService.Authorize(PermissionSystemName.Categories))
                return Forbid();

            return Ok(_categoryApiService.GetCategories());
        }

        [HttpPost]
        public IActionResult Post([FromBody] CategoryDto model)
        {
            if (!_permissionService.Authorize(PermissionSystemName.Categories))
                return Forbid();

            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(model.Id))
                    model = _categoryApiService.InsertCategory(model);
                else
                    model = _categoryApiService.UpdateCategory(model);
                return Created(model);
            }
            return BadRequest(ModelState);
        }

        [HttpDelete]
        public IActionResult Delete(string key)
        {
            if (!_permissionService.Authorize(PermissionSystemName.Categories))
                return Forbid();

            var category = _categoryApiService.GetById(key);
            if (category == null)
            {
                return NotFound();
            }
            _categoryApiService.DeleteCategory(category);
            return Ok();
        }
    }
}
