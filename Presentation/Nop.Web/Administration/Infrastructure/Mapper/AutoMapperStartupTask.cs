﻿using Nop.Core.Infrastructure;

namespace Nop.Admin.Infrastructure.Mapper
{
    /// <summary>
    /// Startup class for AutoMapper
    /// </summary>
    public class AutoMapperStartupTask : IStartupTask
    {
        public void Execute()
        {
            System.Threading.Tasks.Task.Run(() => {
                AutoMapperConfiguration.Init();
            });
        }

        public int Order
        {
            get { return 0; }
        }
    }
}