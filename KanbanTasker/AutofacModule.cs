using Autofac;
using KanbanTasker.Model.Services;
using KanbanTasker.Services;
using KanbanTasker.ViewModels;

namespace KanbanTasker
{
    public class AutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterType<BoardViewModel>();
            builder.RegisterType<MainViewModel>();
            builder.RegisterType<SettingsViewModel>();

            builder.RegisterType<AppNotificationService>()
                .As<IAppNotificationService>()
                .SingleInstance();

            builder.RegisterType<DialogService>()
                .As<IDialogService>()
                .SingleInstance();

            builder.RegisterType<ToastService>()
                .As<IToastService>()
                .SingleInstance();

            builder.RegisterType<NavigationService>()
                .As<INavigationService>()
                .SingleInstance();

            builder.RegisterType<GraphService>()
                .SingleInstance();

            builder.RegisterType<TimerService>()
                .As<ITimerService>()
                .SingleInstance();
        }
    }
}