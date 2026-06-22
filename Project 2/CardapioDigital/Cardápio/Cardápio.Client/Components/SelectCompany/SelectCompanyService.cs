namespace Cardápio.Client.Components.SelectCompany
{
    public class SelectCompanyService
    {
        public bool IsLoading { get; set; } = true;
        private int _empresaSelecionadaID;

        public event Action EmpresaChanged;

        public int EmpresaSelecionada
        {
            get => _empresaSelecionadaID;
            set
            {
                if (_empresaSelecionadaID != value)
                {
                    _empresaSelecionadaID = value;
                    NotifyEmpresaChanged();
                }
            }
        }

        private void NotifyEmpresaChanged() => EmpresaChanged?.Invoke();
    }
}
