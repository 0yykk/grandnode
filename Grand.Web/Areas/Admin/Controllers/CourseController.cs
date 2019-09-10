﻿using Grand.Core;
using Grand.Core.Domain.Courses;
using Grand.Core.Domain.Customers;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security.Authorization;
using Grand.Services.Courses;
using Grand.Services.Customers;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Services.Seo;
using Grand.Services.Stores;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Interfaces;
using Grand.Web.Areas.Admin.Models.Courses;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.Courses)]
    public partial class CourseController : BaseAdminController
    {

        private readonly ILocalizationService _localizationService;
        private readonly ICourseLevelService _courseLevelService;
        private readonly ICourseService _courseService;
        private readonly ICourseSubjectService _courseSubjectService;
        private readonly ICourseLessonService _courseLessonService;
        private readonly ICourseViewModelService _courseViewModelService;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly IStoreService _storeService;
        private readonly ILanguageService _languageService;

        public CourseController(ILocalizationService localizationService, ICourseLevelService courseLevelService, ICourseService courseService,
            ICourseSubjectService courseSubjectService, ICourseLessonService courseLessonService,
            ICourseViewModelService courseViewModelService, IWorkContext workContext, ICustomerService customerService, IStoreService storeService,
            ILanguageService languageService)
        {
            _localizationService = localizationService;
            _courseLevelService = courseLevelService;
            _courseService = courseService;
            _courseSubjectService = courseSubjectService;
            _courseLessonService = courseLessonService;
            _courseViewModelService = courseViewModelService;
            _workContext = workContext;
            _customerService = customerService;
            _storeService = storeService;
            _languageService = languageService;
        }


        #region Level

        public IActionResult Level() => View();

        [HttpPost]
        public async Task<IActionResult> Levels(DataSourceRequest command)
        {
            var levelModel = (await _courseLevelService.GetAll())
                .Select(x => x.ToModel());

            var gridModel = new DataSourceResult {
                Data = levelModel,
                Total = levelModel.Count()
            };
            return Json(gridModel);
        }

        [HttpPost]
        public async Task<IActionResult> LevelUpdate(CourseLevelModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }

            var level = await _courseLevelService.GetById(model.Id);
            level = model.ToEntity(level);
            await _courseLevelService.Update(level);

            return new NullJsonResult();
        }

        [HttpPost]
        public async Task<IActionResult> LevelAdd(CourseLevelModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }

            var level = new CourseLevel();
            level = model.ToEntity(level);
            await _courseLevelService.Insert(level);

            return new NullJsonResult();
        }

        [HttpPost]
        public async Task<IActionResult> LevelDelete(string id)
        {
            var level = await _courseLevelService.GetById(id);
            if (level == null)
                throw new ArgumentException("No level found with the specified id");

            await _courseLevelService.Delete(level);

            return new NullJsonResult();
        }

        #endregion

        #region Course

        public IActionResult List() => View();

        [HttpPost]
        public async Task<IActionResult> List(DataSourceRequest command)
        {
            var courses = await _courseService.GetAll(pageIndex: command.Page - 1, pageSize: command.PageSize);

            var gridModel = new DataSourceResult {
                Data = courses,
                Total = courses.Count
            };
            return Json(gridModel);
        }

        public async Task<IActionResult> Create()
        {
            var model = await _courseViewModelService.PrepareModel();
            //locales
            await AddLocales(_languageService, model.Locales);
            //ACL
            await model.PrepareACLModel(null, false, _customerService);
            //Stores
            await model.PrepareStoresMappingModel(null, _storeService, false, _workContext.CurrentCustomer.StaffStoreId);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Create(CourseModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                if (_workContext.CurrentCustomer.IsStaff())
                {
                    model.LimitedToStores = true;
                    model.SelectedStoreIds = new string[] { _workContext.CurrentCustomer.StaffStoreId };
                }

                var course = await _courseViewModelService.InsertCourseModel(model);
                SuccessNotification(_localizationService.GetResource("Admin.Courses.Course.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = course.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form

            //ACL
            await model.PrepareACLModel(null, true, _customerService);

            ////Stores
            await model.PrepareStoresMappingModel(null, _storeService, true, _workContext.CurrentCustomer.StaffStoreId);

            return View(model);
        }

        public async Task<IActionResult> Edit(string id)
        {
            var course = await _courseService.GetById(id);
            if (course == null)
                //No course found with the specified id
                return RedirectToAction("List");

            if (_workContext.CurrentCustomer.IsStaff())
            {
                if (!course.LimitedToStores || (course.LimitedToStores && course.Stores.Contains(_workContext.CurrentCustomer.StaffStoreId) && course.Stores.Count > 1))
                    WarningNotification(_localizationService.GetResource("Admin.Courses.Course.Permisions"));
                else
                {
                    if (!course.AccessToEntityByStore(_workContext.CurrentCustomer.StaffStoreId))
                        return RedirectToAction("List");
                }
            }

            var model = course.ToModel();
            //locales
            await AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = course.GetLocalized(x => x.Name, languageId, false, false);
                locale.Description = course.GetLocalized(x => x.Description, languageId, false, false);
                locale.MetaKeywords = course.GetLocalized(x => x.MetaKeywords, languageId, false, false);
                locale.MetaDescription = course.GetLocalized(x => x.MetaDescription, languageId, false, false);
                locale.MetaTitle = course.GetLocalized(x => x.MetaTitle, languageId, false, false);
                locale.SeName = course.GetSeName(languageId, false, false);
            });

            model = await _courseViewModelService.PrepareModel(model);
            //ACL
            await model.PrepareACLModel(course, false, _customerService);
            //Stores
            await model.PrepareStoresMappingModel(course, _storeService, false, _workContext.CurrentCustomer.StaffStoreId);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Edit(CourseModel model, bool continueEditing)
        {
            var course = await _courseService.GetById(model.Id);
            if (course == null)
                //No category found with the specified id
                return RedirectToAction("List");

            if (_workContext.CurrentCustomer.IsStaff())
            {
                if (!course.AccessToEntityByStore(_workContext.CurrentCustomer.StaffStoreId))
                    return RedirectToAction("Edit", new { id = course.Id });
            }

            if (ModelState.IsValid)
            {
                if (_workContext.CurrentCustomer.IsStaff())
                {
                    model.LimitedToStores = true;
                    model.SelectedStoreIds = new string[] { _workContext.CurrentCustomer.StaffStoreId };
                }

                course = await _courseViewModelService.UpdateCourseModel(course, model);

                SuccessNotification(_localizationService.GetResource("Admin.Courses.Course.Updated"));
                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabIndex();

                    return RedirectToAction("Edit", new { id = course.Id });
                }
                return RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            model = await _courseViewModelService.PrepareModel(model);
            //ACL
            await model.PrepareACLModel(course, true, _customerService);
            //Stores
            await model.PrepareStoresMappingModel(course, _storeService, true, _workContext.CurrentCustomer.StaffStoreId);

            return View(/*model*/);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var course = await _courseService.GetById(id);
            if (course == null)
                //No category found with the specified id
                return RedirectToAction("List");

            if (_workContext.CurrentCustomer.IsStaff())
            {
                if (!course.AccessToEntityByStore(_workContext.CurrentCustomer.StaffStoreId))
                    return RedirectToAction("Edit", new { id = course.Id });
            }

            if (ModelState.IsValid)
            {
                await _courseViewModelService.DeleteCourse(course);
                SuccessNotification(_localizationService.GetResource("Admin.Courses.Course.Deleted"));
            }
            return RedirectToAction("List");
        }


        #endregion

        #region Subjects

        [HttpPost]
        public async Task<IActionResult> Subjects(DataSourceRequest command, string courseId)
        {
            var subjectModel = (await _courseSubjectService.GetByCourseId(courseId))
                .Select(x => x.ToModel());

            var gridModel = new DataSourceResult {
                Data = subjectModel,
                Total = subjectModel.Count()
            };
            return Json(gridModel);
        }

        [HttpPost]
        public async Task<IActionResult> SubjectUpdate(CourseSubjectModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }

            var subject = await _courseSubjectService.GetById(model.Id);
            subject = model.ToEntity(subject);
            await _courseSubjectService.Update(subject);

            return new NullJsonResult();
        }

        [HttpPost]
        public async Task<IActionResult> SubjectAdd(CourseSubjectModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }

            var subject = new CourseSubject();
            subject = model.ToEntity(subject);
            await _courseSubjectService.Insert(subject);

            return new NullJsonResult();
        }

        [HttpPost]
        public async Task<IActionResult> SubjectDelete(string id)
        {
            var subject = await _courseSubjectService.GetById(id);
            if (subject == null)
                throw new ArgumentException("No subject found with the specified id");

            await _courseSubjectService.Delete(subject);

            return new NullJsonResult();
        }

        #endregion

        #region Lessons

        [HttpPost]
        public async Task<IActionResult> Lessons(string courseId)
        {
            var lessonModel = (await _courseLessonService.GetByCourseId(courseId))
                .Select(x => x.ToModel());

            var gridModel = new DataSourceResult {
                Data = lessonModel,
                Total = lessonModel.Count()
            };
            return Json(gridModel);
        }
        #endregion
    }
}
