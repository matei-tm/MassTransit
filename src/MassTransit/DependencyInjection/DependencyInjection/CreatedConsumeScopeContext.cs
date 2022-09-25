﻿namespace MassTransit.DependencyInjection
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;


    public class CreatedConsumeScopeContext :
        IConsumeScopeContext
    {
        readonly IServiceScope _scope;

        public CreatedConsumeScopeContext(IServiceScope scope, ConsumeContext context)
        {
            _scope = scope;
            Context = context;
        }

        public ConsumeContext Context { get; }

        public ValueTask DisposeAsync()
        {
            if (_scope is IAsyncDisposable asyncDisposable)
                return asyncDisposable.DisposeAsync();

            _scope?.Dispose();
            return default;
        }
    }


    public class CreatedConsumeScopeContext<TMessage> :
        IConsumeScopeContext<TMessage>
        where TMessage : class
    {
        readonly IServiceScope _scope;

        public CreatedConsumeScopeContext(IServiceScope scope, ConsumeContext<TMessage> context)
        {
            _scope = scope;
            Context = context;
        }

        public ConsumeContext<TMessage> Context { get; }

        public T GetService<T>()
            where T : class
        {
            return ActivatorUtilities.GetServiceOrCreateInstance<T>(_scope.ServiceProvider);
        }

        public T CreateInstance<T>(params object[] arguments)
            where T : class
        {
            return ActivatorUtilities.CreateInstance<T>(_scope.ServiceProvider, arguments);
        }

        public IDisposable PushConsumeContext(ConsumeContext context)
        {
            return _scope.ServiceProvider.GetRequiredService<ScopedConsumeContextProvider>().PushContext(context);
        }

        public ValueTask DisposeAsync()
        {
            if (_scope is IAsyncDisposable asyncDisposable)
                return asyncDisposable.DisposeAsync();

            _scope?.Dispose();
            return default;
        }
    }
}
