namespace Cardápio.Client.Components.Modal
{
    public class ModalService
    {
        private readonly Dictionary<string, ModalState> _modals = new();
        public event Action? OnStateChange;
        public List<object> ObjectList;
        public object? EditObject;

        public void OpenModal(
            string modalId,
            Func<Task>? onConfirm = null,
            Func<object?, Task>? onConfirmWithParameter = null,
            Func<Task>? onCancel = null,
            object? editObject = null)
        {
            if (!_modals.ContainsKey(modalId))
            {
                _modals[modalId] = new ModalState();
            }

            var modal = _modals[modalId];
            modal.IsOpen = true;
            modal.OnConfirm = onConfirm;
            modal.OnConfirmWithParameter = onConfirmWithParameter;
            modal.OnCancel = onCancel;
            modal.EditObject = editObject;

            NotifyStateChanged();
        }

        public void CloseModal(string modalId)
        {
            if (_modals.TryGetValue(modalId, out var modal))
            {
                modal.IsOpen = false;
                _modals.Remove(modalId);
                NotifyStateChanged();
            }
        }

        public ModalState? GetModalState(string modalId) =>
            _modals.TryGetValue(modalId, out var modal) ? modal : null;

        private void NotifyStateChanged() => OnStateChange?.Invoke();
    }

    public class ModalState
    {
        public bool IsOpen { get; set; }
        public Func<Task>? OnConfirm { get; set; }
        public Func<object?, Task>? OnConfirmWithParameter { get; set; }
        public Func<Task>? OnCancel { get; set; }
        public object? EditObject { get; set; }
    }
}
