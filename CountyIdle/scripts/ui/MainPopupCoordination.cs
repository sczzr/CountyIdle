using Godot;

namespace CountyIdle;

public partial class Main
{
    private bool IsBlockingOverlayPopupOpen()
    {
        return _settingsPanel?.Visible == true ||
               _warehousePanel?.Visible == true ||
               _taskPanel?.Visible == true ||
               _disciplePanel?.Visible == true ||
               _sectOrganizationPanel?.Visible == true ||
               _saveSlotsPanel?.Visible == true;
    }

    private void CloseBlockingOverlayPopups(Control? except = null)
    {
        if (_settingsPanel != null && _settingsPanel.Visible && !ReferenceEquals(_settingsPanel, except))
        {
            _settingsPanel.ClosePanel();
        }

        if (_warehousePanel != null && _warehousePanel.Visible && !ReferenceEquals(_warehousePanel, except))
        {
            _warehousePanel.ClosePanel();
        }

        if (_taskPanel != null && _taskPanel.Visible && !ReferenceEquals(_taskPanel, except))
        {
            _taskPanel.ClosePanel();
        }

        if (_disciplePanel != null && _disciplePanel.Visible && !ReferenceEquals(_disciplePanel, except))
        {
            _disciplePanel.ClosePanel();
        }

        if (_sectOrganizationPanel != null && _sectOrganizationPanel.Visible && !ReferenceEquals(_sectOrganizationPanel, except))
        {
            _sectOrganizationPanel.ClosePanel();
        }

        if (_saveSlotsPanel != null && _saveSlotsPanel.Visible && !ReferenceEquals(_saveSlotsPanel, except))
        {
            _saveSlotsPanel.ClosePanel();
        }
    }
}
