﻿using Nop.Services.Customers;
using Nop.Core.Domain.Tasks;

namespace Nop.Services.Tasks
{
    public partial class CustomerReminderLastActivityScheduleTask : ScheduleTask, IScheduleTask
    {
        private readonly ICustomerReminderService _customerReminderService;

        public CustomerReminderLastActivityScheduleTask(ICustomerReminderService customerReminderService)
        {
            this._customerReminderService = customerReminderService;
        }

        public void Execute()
        {
            _customerReminderService.Task_LastActivity();
        }
    }
}
