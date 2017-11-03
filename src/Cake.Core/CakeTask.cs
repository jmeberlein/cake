﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Cake.Core
{
    /// <summary>
    /// A <see cref="CakeTask"/> represents a unit of work.
    /// </summary>
    public abstract class CakeTask : ICakeTaskInfo
    {
        private readonly List<CakeTaskDependency> _dependencies;
        private readonly List<CakeTaskDependency> _reverseDependencies;
        private readonly List<Func<ICakeContext, bool>> _criterias;

        /// <summary>
        /// Gets the name of the task.
        /// </summary>
        /// <value>The name of the task.</value>
        public string Name { get; }

        /// <summary>
        /// Gets or sets the description of the task.
        /// </summary>
        /// <value>The description of the task.</value>
        public string Description { get; set; }

        /// <summary>
        /// Gets the task's dependencies.
        /// </summary>
        /// <value>The task's dependencies.</value>
        public IReadOnlyList<CakeTaskDependency> Dependencies => _dependencies;

        /// <summary>
        /// Gets the tasks that the task want to be a dependency of.
        /// </summary>
        /// <value>The tasks that the task want to be a dependency of.</value>
        public IReadOnlyList<CakeTaskDependency> Dependees => _reverseDependencies;

        /// <summary>
        /// Gets the task's criterias.
        /// </summary>
        /// <value>The task's criterias.</value>
        public IReadOnlyList<Func<ICakeContext, bool>> Criterias => _criterias;

        /// <summary>
        /// Gets the error handler.
        /// </summary>
        /// <value>The error handler.</value>
        public Action<Exception> ErrorHandler { get; private set; }

        /// <summary>
        /// Gets the error reporter.
        /// </summary>
        public Action<Exception> ErrorReporter { get; private set; }

        /// <summary>
        /// Gets the finally handler.
        /// </summary>
        public Action FinallyHandler { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CakeTask"/> class.
        /// </summary>
        /// <param name="name">The name of the task.</param>
        protected CakeTask(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Task name cannot be empty.");
            }

            _dependencies = new List<CakeTaskDependency>();
            _reverseDependencies = new List<CakeTaskDependency>();
            _criterias = new List<Func<ICakeContext, bool>>();

            Name = name;
        }

        /// <summary>
        /// Adds a dependency to the task.
        /// </summary>
        /// <param name="name">The name of the dependency.</param>
        /// <param name="required">Whether or not the dependency is required.</param>
        /// <param name="predependency">Whether the dependency is a predependency or postdependency.</param>
        public void AddDependency(string name, bool required = true, bool predependency = true)
        {
            if (_dependencies.Any(x => x.Name == name))
            {
                const string format = "The task '{0}' already have a dependency on '{1}'.";
                var message = string.Format(CultureInfo.InvariantCulture, format, Name, name);
                throw new CakeException(message);
            }
            _dependencies.Add(new CakeTaskDependency(name, required, predependency));
        }

        /// <summary>
        /// Makes this task a dependency of some other task.
        /// </summary>
        /// <param name="name">The name of the task that this task want to be a dependency of.</param>
        /// <param name="required">Whether or not the dependency is required.</param>
        /// <param name="predependency">Whether the dependency is a predependency or postdependency.</param>
        public void AddReverseDependency(string name, bool required = true, bool predependency = true)
        {
            if (_reverseDependencies.Any(x => x.Name == name))
            {
                const string format = "The task '{0}' already is a dependee of '{1}'.";
                var message = string.Format(CultureInfo.InvariantCulture, format, Name, name);
                throw new CakeException(message);
            }
            _reverseDependencies.Add(new CakeTaskDependency(name, required, predependency));
        }

        /// <summary>
        /// Adds a criteria to the task that is invoked when the task is invoked.
        /// </summary>
        /// <param name="criteria">The criteria.</param>
        public void AddCriteria(Func<ICakeContext, bool> criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException(nameof(criteria));
            }
            _criterias.Add(criteria);
        }

        /// <summary>
        /// Sets the error handler for the task.
        /// The error handler is invoked when an exception is thrown from the task.
        /// </summary>
        /// <param name="errorHandler">The error handler.</param>
        public void SetErrorHandler(Action<Exception> errorHandler)
        {
            if (errorHandler == null)
            {
                throw new ArgumentNullException(nameof(errorHandler));
            }
            if (ErrorHandler != null)
            {
                throw new CakeException("There can only be one error handler per task.");
            }
            ErrorHandler = errorHandler;
        }

        /// <summary>
        /// Sets the error reporter for the task.
        /// The error reporter is invoked when an exception is thrown from the task.
        /// This action is invoked before the error handler, but gives no opportunity to recover from the error.
        /// </summary>
        /// <param name="errorReporter">The error reporter.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="errorReporter"/> is <c>null</c>.</exception>
        /// <exception cref="CakeException">There can only be one error reporter per task.</exception>
        public void SetErrorReporter(Action<Exception> errorReporter)
        {
            if (errorReporter == null)
            {
                throw new ArgumentNullException(nameof(errorReporter));
            }
            if (ErrorReporter != null)
            {
                throw new CakeException("There can only be one error reporter per task.");
            }
            ErrorReporter = errorReporter;
        }

        /// <summary>
        /// Sets the finally handler for the task.
        /// The finally handler is always invoked when a task have finished running.
        /// </summary>
        /// <param name="finallyHandler">The finally handler.</param>
        public void SetFinallyHandler(Action finallyHandler)
        {
            if (finallyHandler == null)
            {
                throw new ArgumentNullException(nameof(finallyHandler));
            }
            if (FinallyHandler != null)
            {
                throw new CakeException("There can only be one finally handler per task.");
            }
            FinallyHandler = finallyHandler;
        }

        /// <summary>
        /// Executes the task using the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>Returned Task</returns>
        public abstract Task Execute(ICakeContext context);
    }
}