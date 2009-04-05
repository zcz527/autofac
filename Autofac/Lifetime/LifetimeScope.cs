﻿using System;
using System.Collections.Generic;
using Autofac.Resolving;
using Autofac.Registry;
using Autofac.Disposal;
using Autofac.Services;
using Autofac.SelfRegistration;

namespace Autofac.Lifetime
{
    public class LifetimeScope : Disposable, ISharingLifetimeScope
    {
        IComponentRegistry _componentRegistry;
        ISharingLifetimeScope _root; // Root optimises singleton lookup without traversal
        ISharingLifetimeScope _parent;
        IDisposer _disposer = new Disposer();
        IDictionary<Guid, object> _sharedInstances = new Dictionary<Guid, object>();
        object _tag;

        protected LifetimeScope(IComponentRegistry componentRegistry, LifetimeScope parent)
            : this(componentRegistry)
        {
            _parent = Enforce.ArgumentNotNull(parent, "parent");
            _root = _parent.RootLifetimeScope;
        }

        public LifetimeScope(IComponentRegistry componentRegistry)
        {
            _componentRegistry = Enforce.ArgumentNotNull(componentRegistry, "componentRegistry");
            _root = this;
        }

        public ILifetimeScope BeginLifetimeScope()
        {
            var result = new LifetimeScope(_componentRegistry, this);
            result.Resolve<IndirectReference<ILifetimeScope>>().Value = result;
            return result;
        }

        public object Resolve(IComponentRegistration registration, IEnumerable<Parameter> parameters)
        {
            var operation = new ResolveOperation(this);
            return operation.Resolve(registration, parameters);
        }

        public ISharingLifetimeScope ParentLifetimeScope
        {
            get { return _parent; }
        }

        public ISharingLifetimeScope RootLifetimeScope
        {
            get { return _root; }
        }

        public bool TryGetSharedInstance(Guid id, out object result)
        {
            return _sharedInstances.TryGetValue(id, out result);
        }

        public void AddSharedInstance(Guid id, object newInstance)
        {
            Enforce.ArgumentNotNull(newInstance, "newInstance");

            _sharedInstances.Add(id, newInstance);
        }

        public IDisposer Disposer
        {
            get { return _disposer; }
        }

        public object Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }

        public IComponentRegistry ComponentRegistry
        {
            get { return _componentRegistry; }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _disposer.Dispose();

            base.Dispose(disposing);
        }
    }
}