using System;
using System.Collections.Generic;
using System.Text;
using Autofac;
using KanbanTasker.Model;

namespace KanbanTasker.Services
{
    public class AutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);
        }
    }
}