// Form1.cs - Versão final completa
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Calibrador
{
    public partial class Form1 : Form
    {
        #region Private Fields

        private IConnectionManager _connectionManager;
        private IDataReader _dataReader;
        private IValidator _validator;
        private ILogger _logger;
        private IUiStateManager _uiStateManager;
        private LogManager _logManager;
        private Timer _measurementTimer;

        #endregion

        #region Public Properties - Acesso aos controles

        public Button BtnConectar => btnConectar;
        public Button BtnDesconectar => btnDesconectar;
        public Button BtnCalibrarEscala => btnCalibrarEscala;
        public Label LblTensaoDut => lblTensaoDut;
        public Label LblTensaoDutVB => lblTensaoDutVB;
        public Label LblTensaoDutVC => lblTensaoDutVC;
        public Label LblTensaoDutVN => lblTensaoDutVN;
        public Label LblCorrenteDut => lblCorrenteDut;
        public Label LblCorrenteDutB => lblCorrenteDutB;
        public Label LblCorrenteDutC => lblCorrenteDutC;
        public Label LblCorrenteDutN => lblCorrenteDutN;
        public TextBox TxtIpDut => txtIpDut;
        public RichTextBox RtbLog => rtbLog;

        #endregion

        #region Constructor

        public Form1()
        {
            InitializeComponent();
            InitializeDependencies();
            SetupEventHandlers();
            InitializeTimer();
            InitializeApplication();
        }

        private void InitializeDependencies()
        {
            var modbusService = new ModbusRtuOverTcpService();
            _logger = new Logger();
            _connectionManager = new ModbusConnectionManager(modbusService, _logger);
            _dataReader = new ModbusDataReader(modbusService, _logger);
            _validator = new NetworkValidator();

            _uiStateManager = new UiStateManager(this, _logger);
            _logManager = new LogManager(rtbLog);
        }

        private void SetupEventHandlers()
        {
            _connectionManager.ConnectionStatusChanged += OnConnectionStatusChanged;
            _logger.LogEntryAdded += OnLogEntryAdded;
        }

        private void InitializeTimer()
        {
            _measurementTimer = new Timer
            {
                Interval = 1000,
                Enabled = false
            };
            _measurementTimer.Tick += OnMeasurementTimerTick;
        }

        private void InitializeApplication()
        {
            _uiStateManager.SetDisconnected();
            _logger.LogStartup();
        }

        #endregion

        #region Event Handlers - Botões

        private async void btnConectar_Click(object sender, EventArgs e)
        {
            await ExecuteWithErrorHandling(ConnectAsync, "Erro durante conexão");
        }

        private async void btnDesconectar_Click(object sender, EventArgs e)
        {
            await ExecuteWithErrorHandling(DisconnectAsync, "Erro durante desconexão");
        }

        private async void OnMeasurementTimerTick(object sender, EventArgs e)
        {
            await ExecuteWithErrorHandling(ReadMeasurementsAsync, "Erro na leitura");
        }

        #endregion

        #region Event Handlers - Serviços

        private void OnConnectionStatusChanged(object sender, ConnectionStatusEventArgs e)
        {
            _uiStateManager.UpdateConnectionStatus(e.IsConnected, e.Message, e.FirmwareVersion);

            if (e.IsConnected)
            {
                _measurementTimer.Start();
            }
            else
            {
                _measurementTimer.Stop();
            }
        }

        private void OnLogEntryAdded(object sender, LogEventArgs e)
        {
            _logManager.AppendLog(e);
        }

        #endregion

        #region Core Business Logic

        private async Task ConnectAsync()
        {
            _uiStateManager.SetConnecting();

            var validation = _validator.ValidateIpAddress(txtIpDut.Text);
            if (!validation.IsValid)
            {
                _uiStateManager.ShowWarning(validation.ErrorMessage);
                txtIpDut.Focus();
                _uiStateManager.SetIdle();
                return;
            }

            string ipAddress = txtIpDut.Text.Trim();
            var result = await _connectionManager.ConnectAsync(ipAddress, 1001);

            if (result.Success)
            {
                await OnConnectionSuccessful(result);
            }
            else
            {
                OnConnectionFailed(result);
            }
        }

        private async Task DisconnectAsync()
        {
            await _connectionManager.DisconnectAsync();
            _uiStateManager.ShowSuccess("Desconectado com sucesso!");
        }

        private async Task ReadMeasurementsAsync()
        {
            if (!_connectionManager.IsConnected)
            {
                return;
            }

            try
            {
                var measurements = await _dataReader.ReadMeasurementsAsync();
                _uiStateManager.UpdateMeasurementDisplay(measurements);
            }
            catch (Exception ex)
            {
                HandleMeasurementError(ex);
            }
        }

        #endregion

        #region Business Logic Helpers

        private async Task OnConnectionSuccessful(ConnectionResult result)
        {
            _uiStateManager.SetConnected(result.FirmwareVersion);
            await Task.Delay(100);

            var successMessage = "Conectado com sucesso!\n\n";
            successMessage += $"IP: {txtIpDut.Text.Trim()}\n";

            if (result.FirmwareVersion > 0)
            {
                successMessage += $"Firmware: v{result.FirmwareVersion:F2}";
            }

            _uiStateManager.ShowSuccess(successMessage);
        }

        private void OnConnectionFailed(ConnectionResult result)
        {
            _uiStateManager.SetDisconnected();
            var errorMessage = BuildConnectionErrorMessage(result);
            _uiStateManager.ShowError(errorMessage);
        }

        private void HandleMeasurementError(Exception ex)
        {
            _logger.Warning($"Erro leitura: {ex.Message}");

            if (IsConnectionLostError(ex))
            {
                _uiStateManager.SetDisconnected();
                _uiStateManager.ShowWarning("Conexão perdida. Reconecte para continuar.");
            }
        }

        #endregion

        #region Utility Methods

        private async Task ExecuteWithErrorHandling(Func<Task> operation, string errorContext)
        {
            try
            {
                await operation();
            }
            catch (Exception ex)
            {
                _logger.Error($"{errorContext}: {ex.Message}", ex);
                _uiStateManager.ShowError($"{errorContext}\n\n{ex.Message}");
                _uiStateManager.SetIdle();
            }
        }

        private string BuildConnectionErrorMessage(ConnectionResult result)
        {
            var baseMessage = $"Erro ao conectar:\n\n{result.Message}";

            if (result.Exception != null)
                baseMessage += $"\n\nDetalhes: {result.Exception.Message}";

            baseMessage += $"\n\nVerifique:\n";
            baseMessage += $"• IP está correto\n";
            baseMessage += $"• Equipamento ligado e na rede\n";
            baseMessage += $"• Porta 1001 liberada no firewall";

            return baseMessage;
        }

        private bool IsConnectionLostError(Exception ex)
        {
            return ex.Message.Contains("não está conectado") ||
                   ex.Message.Contains("conexão") ||
                   ex is System.IO.IOException;
        }

        #endregion

        #region Form Lifecycle

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                _logger.Info("Encerrando aplicação");

                _measurementTimer?.Stop();
                _measurementTimer?.Dispose();

                Task.Run(async () =>
                {
                    try
                    {
                        if (_connectionManager?.IsConnected == true)
                        {
                            await _connectionManager.DisconnectAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Erro ao desconectar: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao fechar: {ex.Message}");
            }
        }

        #endregion


    }
}