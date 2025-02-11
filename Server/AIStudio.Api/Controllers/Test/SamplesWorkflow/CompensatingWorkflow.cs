﻿using AIStudio.Common.DI;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace AIStudio.Api.Controllers.Test.SamplesWorkflow
{
    /// <summary>
    /// CompensatingWorkflow
    /// </summary>
    /// <seealso cref="WorkflowCore.Interface.IWorkflow" />
    class CompensatingWorkflow : IWorkflow
    {
        /// <summary>
        /// The logger
        /// </summary>
        private readonly ILogger<CompensatingWorkflow> _logger;
        /// <summary>
        /// CompensatingWorkflow
        /// </summary>
        /// <param name="logger">The logger.</param>
        public CompensatingWorkflow(ILogger<CompensatingWorkflow> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public string Id => "compensate-sample";
        /// <summary>
        /// Gets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public int Version => 1;

        /// <summary>
        /// Builds the specified builder.
        /// </summary>
        /// <param name="builder">The builder.</param>
        public void Build(IWorkflowBuilder<object> builder)
        {
           
            builder
                .StartWith(context => _logger.LogInformation("Begin"))
                .Saga(saga => saga
                    .StartWith<Task1>()
                        .CompensateWith<UndoTask1>()
                    //Task2 一直在抛异常，导致任务一直重试
                    .Then<Task2>()
                        .CompensateWith<UndoTask2>()
                    .Then<Task3>()
                        .CompensateWith<UndoTask3>()
                )
                    .OnError(WorkflowCore.Models.WorkflowErrorHandling.Retry, TimeSpan.FromSeconds(5))
                .Then(context => _logger.LogInformation("End"));
        }
    }

    /// <summary>
    /// Task1
    /// </summary>
    /// <seealso cref="WorkflowCore.Models.StepBody" />
    /// <seealso cref="AIStudio.Common.DI.ITransientDependency" />
    public class Task1 : StepBody, ITransientDependency
    {
        /// <summary>
        /// The logger
        /// </summary>
        private readonly ILogger<Task1> _logger;
        /// <summary>
        /// Task1
        /// </summary>
        /// <param name="logger">The logger.</param>
        public Task1(ILogger<Task1> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Run
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public override ExecutionResult Run(IStepExecutionContext context)
        {
            _logger.LogInformation("Doing Task 1");
            return ExecutionResult.Next();
        }
    }

    /// <summary>
    /// Task2
    /// </summary>
    /// <seealso cref="WorkflowCore.Models.StepBody" />
    /// <seealso cref="AIStudio.Common.DI.ITransientDependency" />
    public class Task2 : StepBody, ITransientDependency
    {
        /// <summary>
        /// The logger
        /// </summary>
        private readonly ILogger<Task2> _logger;
        /// <summary>
        /// Task2
        /// </summary>
        /// <param name="logger">The logger.</param>
        public Task2(ILogger<Task2> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Run
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        /// <exception cref="AIStudio.Api.Controllers.Test.ValidationTestController.Exception"></exception>
        /// <exception cref="Exception"></exception>
        public override ExecutionResult Run(IStepExecutionContext context)
        {
            _logger.LogInformation("Doing Task 2");
            throw new Exception();
        }
    }

    /// <summary>
    /// Task3
    /// </summary>
    /// <seealso cref="WorkflowCore.Models.StepBody" />
    /// <seealso cref="AIStudio.Common.DI.ITransientDependency" />
    public class Task3 : StepBody, ITransientDependency
    {
        /// <summary>
        /// The logger
        /// </summary>
        private readonly ILogger<Task3> _logger;
        /// <summary>
        /// Task3
        /// </summary>
        /// <param name="logger">The logger.</param>
        public Task3(ILogger<Task3> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Run
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public override ExecutionResult Run(IStepExecutionContext context)
        {
            _logger.LogInformation("Doing Task 3");
            return ExecutionResult.Next();
        }
    }

    /// <summary>
    /// UndoTask1
    /// </summary>
    /// <seealso cref="WorkflowCore.Models.StepBody" />
    /// <seealso cref="AIStudio.Common.DI.ITransientDependency" />
    public class UndoTask1 : StepBody, ITransientDependency
    {
        /// <summary>
        /// The logger
        /// </summary>
        private readonly ILogger<UndoTask1> _logger;
        /// <summary>
        /// UndoTask1
        /// </summary>
        /// <param name="logger">The logger.</param>
        public UndoTask1(ILogger<UndoTask1> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Runs the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public override ExecutionResult Run(IStepExecutionContext context)
        {
            _logger.LogInformation("Undoing Task 1");
            return ExecutionResult.Next();
        }
    }

    /// <summary>
    /// UndoTask2
    /// </summary>
    /// <seealso cref="WorkflowCore.Models.StepBody" />
    /// <seealso cref="AIStudio.Common.DI.ITransientDependency" />
    public class UndoTask2 : StepBody, ITransientDependency
    {
        /// <summary>
        /// The logger
        /// </summary>
        private readonly ILogger<UndoTask2> _logger;
        /// <summary>
        /// UndoTask2
        /// </summary>
        /// <param name="logger">The logger.</param>
        public UndoTask2(ILogger<UndoTask2> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Runs the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public override ExecutionResult Run(IStepExecutionContext context)
        {
            _logger.LogInformation("Undoing Task 2");
            return ExecutionResult.Next();
        }
    }

    /// <summary>
    /// UndoTask3
    /// </summary>
    /// <seealso cref="WorkflowCore.Models.StepBody" />
    /// <seealso cref="AIStudio.Common.DI.ITransientDependency" />
    public class UndoTask3 : StepBody, ITransientDependency
    {
        /// <summary>
        /// The logger
        /// </summary>
        private readonly ILogger<UndoTask3> _logger;
        /// <summary>
        /// UndoTask3
        /// </summary>
        /// <param name="logger">The logger.</param>
        public UndoTask3(ILogger<UndoTask3> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Run
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public override ExecutionResult Run(IStepExecutionContext context)
        {
            _logger.LogInformation("Undoing Task 3");
            return ExecutionResult.Next();
        }
    }
}