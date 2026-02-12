using Microsoft.AspNetCore.Components;

namespace AcademicManager.Web.Components.Wizards
{
    public class WizardStep<T>
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = "";
        public string? Subtitle { get; set; }
        public string Description { get; set; } = "";
        public string Icon { get; set; } = "";
        public bool IsOptional { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsCurrent { get; set; }
        public bool ShowNextButton { get; set; } = true;
        public bool ShowFinishButton { get; set; } = true;
        public RenderFragment? Content { get; set; }
        public Func<T, Task<bool>>? ValidateAsync { get; set; }
        public Func<T, bool>? Validate { get; set; }
        public Func<T, Task<bool>>? OnNext { get; set; }
        public Func<T, Task<bool>>? OnPrevious { get; set; }
        public Func<T, Task<bool>>? OnFinish { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    public class WizardStepChangeEventArgs<T>
    {
        public WizardStep<T>? CurrentStep { get; set; }
        public WizardStep<T>? PreviousStep { get; set; }
        public int CurrentStepIndex { get; set; }
        public int PreviousStepIndex { get; set; }
        public int NewStepIndex { get; set; }
        public int OldStepIndex { get; set; }
        public bool IsForward { get; set; }
        public bool IsCompleted { get; set; }
        public T Model { get; set; } = default!;
        public WizardStepDirection Direction { get; set; }
    }

    public enum WizardStepDirection
    {
        Forward,
        Backward,
        Jump
    }
}
