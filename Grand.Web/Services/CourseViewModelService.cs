﻿using Grand.Core;
using Grand.Core.Domain.Courses;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Media;
using Grand.Services.Courses;
using Grand.Services.Media;
using Grand.Services.Orders;
using Grand.Web.Extensions;
using Grand.Web.Interfaces;
using Grand.Web.Models.Course;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Services
{
    public partial class CourseViewModelService : ICourseViewModelService
    {
        private readonly ICourseService _courseService;
        private readonly ICourseSubjectService _courseSubjectService;
        private readonly ICourseLessonService _courseLessonService;
        private readonly ICourseLevelService _courseLevelService;
        private readonly ICourseActionService _courseActionService;
        private readonly IOrderService _orderService;
        private readonly IWorkContext _workContext;
        private readonly IPictureService _pictureService;
        private readonly MediaSettings _mediaSettings;

        public CourseViewModelService(ICourseService courseService,
            ICourseSubjectService courseSubjectService,
            ICourseLessonService courseLessonService,
            ICourseLevelService courseLevelService,
            ICourseActionService courseActionService,
            IOrderService orderService, IWorkContext workContext,
            IPictureService pictureService,
            MediaSettings mediaSettings)
        {
            _courseService = courseService;
            _courseSubjectService = courseSubjectService;
            _courseLessonService = courseLessonService;
            _courseLevelService = courseLevelService;
            _courseActionService = courseActionService;
            _orderService = orderService;
            _workContext = workContext;
            _pictureService = pictureService;
            _mediaSettings = mediaSettings;
        }

        public virtual Task<Course> GetCourseById(string courseId)
        {
            return _courseService.GetById(courseId);
        }

        public virtual async Task<bool> CheckOrder(Course course, Customer customer)
        {
            if (string.IsNullOrEmpty(course.ProductId))
                return true;

            var orders = await _orderService.SearchOrders(customerId: customer.Id, productId: course.ProductId, ps: Core.Domain.Payments.PaymentStatus.Paid);
            if (orders.TotalCount > 0)
                return true;

            return false;
        }

        public virtual async Task<CourseModel> PrepareCourseModel(Course course)
        {
            var model = course.ToModel(_workContext.WorkingLanguage);
            model.Level = (await _courseLevelService.GetById(course.LevelId))?.Name;

            var pictureSize = _mediaSettings.CourseThumbPictureSize;
            var picture = await _pictureService.GetPictureById(course.PictureId);
            model.PictureUrl = await _pictureService.GetPictureUrl(picture, pictureSize);

            var subjects = await _courseSubjectService.GetByCourseId(course.Id);
            foreach (var item in subjects)
            {
                model.Subjects.Add(new CourseModel.Subject() {
                    Id = item.Id,
                    Name = item.Name,
                    DisplayOrder = item.DisplayOrder
                });
            }

            var lessonPictureSize = _mediaSettings.LessonThumbPictureSize;
            var lessons = await _courseLessonService.GetByCourseId(course.Id);
            foreach (var item in lessons.Where(x => x.Published))
            {
                var lessonPicture = await _pictureService.GetPictureById(item.PictureId);
                var pictureUrl = await _pictureService.GetPictureUrl(lessonPicture, lessonPictureSize);
                var approved = await _courseActionService.CustomerLessonCompleted(_workContext.CurrentCustomer.Id, item.Id);

                model.Lessons.Add(new CourseModel.Lesson() {
                    Id = item.Id,
                    SubjectId = item.SubjectId,
                    Name = item.Name,
                    ShortDescription = item.ShortDescription,
                    DisplayOrder = item.DisplayOrder,
                    PictureUrl = pictureUrl,
                    Approved = approved
                });
            }
            model.Approved = !model.Lessons.Any(x => !x.Approved);
            return model;
        }
    }
}
