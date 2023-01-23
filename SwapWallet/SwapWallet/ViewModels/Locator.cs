using System;
using Autofac;
using SwapWallet.Services;
using VicWeb.Lifi;

namespace SwapWallet.ViewModels
{
    public class Locator
    {
        private IContainer _container;
        private readonly ContainerBuilder _containerBuilder;

        private static readonly Locator _instance = new Locator();
        public static Locator Instance => _instance;

        public Locator()
        {
            _containerBuilder = new ContainerBuilder();
            _containerBuilder.RegisterType<AuthenticationService>().SingleInstance().As<IAuthenticationService>();
            _containerBuilder.RegisterType<FileService>().SingleInstance().As<IFileService>();
            _containerBuilder.RegisterType<UserService>().SingleInstance().As<IUserService>();
            _containerBuilder.RegisterType<MetaService>().SingleInstance().As<IMetaService>();
            _containerBuilder.RegisterType<LifiService>().SingleInstance().As<ILifiService>();
            _containerBuilder.RegisterType<Web3Provider>().SingleInstance().As<IWeb3Provider>();
        }

        public T Resolve<T>()
        {
            return _container.Resolve<T>();
        }

        public object Resolve(Type type)
        {
            return _container.Resolve(type);
        }

        public void Register<TInterface, TImplementation>() where TImplementation : TInterface
        {
            _containerBuilder.RegisterType<TImplementation>().As<TInterface>();
        }

        public void Register<T>() where T : class
        {
            _containerBuilder.RegisterType<T>();
        }

        public void Build()
        {
            if (_container != null) return;
            _container = _containerBuilder.Build();
        }
    }
}
