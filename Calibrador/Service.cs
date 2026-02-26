// Services.cs - Versão limpa e simplificada
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Windows.Forms;
using System.Linq;
using System.Globalization;

namespace Calibrador
{
    // ===== LOGGER SIMPLES =====
    public enum LogLevel
    {
        Info,
        Warning,
        Error
    }

    public class LogEventArgs : EventArgs
    {
        public DateTime Timestamp { get; set; }
        public LogLevel Level { get; set; }
        public string Message { get; set; }
        public Exception Exception { get; set; }
    }

    public interface ILogger
    {
        void Info(string message);
        void Warning(string message);
        void Error(string message, Exception exception = null);
        void LogStartup();
        event EventHandler<LogEventArgs> LogEntryAdded;
    }

    public class Logger : ILogger
    {
        public event EventHandler<LogEventArgs> LogEntryAdded;

        public void Info(string message) => Log(LogLevel.Info, message);
        public void Warning(string message) => Log(LogLevel.Warning, message);
        public void Error(string message, Exception exception = null) => Log(LogLevel.Error, message, exception);

        public void LogStartup()
        {
            var now = DateTime.Now;
            Info("CALIBRADOR INICIADO");
            
        }

        private void Log(LogLevel level, string message, Exception exception = null)
        {
            var logEntry = new LogEventArgs
            {
                Timestamp = DateTime.Now,
                Level = level,
                Message = message,
                Exception = exception
            };

            LogEntryAdded?.Invoke(this, logEntry);
        }
    }

    // ===== VALIDATOR =====
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }

        public static ValidationResult Success() => new ValidationResult { IsValid = true };
        public static ValidationResult Failure(string message) => new ValidationResult { IsValid = false, ErrorMessage = message };
    }

    public interface IValidator
    {
        ValidationResult ValidateIpAddress(string ipAddress);
    }

    public class NetworkValidator : IValidator
    {
        public ValidationResult ValidateIpAddress(string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
                return ValidationResult.Failure("IP é obrigatório");

            if (!IPAddress.TryParse(ipAddress.Trim(), out var _))
                return ValidationResult.Failure("Formato de IP inválido");

            return ValidationResult.Success();
        }
    }

    // ===== CONNECTION MANAGER =====
    public class ConnectionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public float FirmwareVersion { get; set; }
        public Exception Exception { get; set; }

        public static ConnectionResult CreateSuccess(string message, float firmwareVersion) =>
            new ConnectionResult { Success = true, Message = message, FirmwareVersion = firmwareVersion };

        public static ConnectionResult CreateFailure(string message, Exception ex = null) =>
            new ConnectionResult { Success = false, Message = message, Exception = ex };
    }

    public class ConnectionStatusEventArgs : EventArgs
    {
        public bool IsConnected { get; set; }
        public string Message { get; set; }
        public float FirmwareVersion { get; set; }
    }

    public interface IConnectionManager
    {
        bool IsConnected { get; }
        Task<ConnectionResult> ConnectAsync(string ipAddress, int port);
        Task DisconnectAsync();
        event EventHandler<ConnectionStatusEventArgs> ConnectionStatusChanged;
    }

    public class ModbusConnectionManager : IConnectionManager
    {
        private readonly ModbusRtuOverTcpService _modbusService;
        private readonly ILogger _logger;

        public bool IsConnected => _modbusService?.IsConnected ?? false;
        public event EventHandler<ConnectionStatusEventArgs> ConnectionStatusChanged;

        public ModbusConnectionManager(ModbusRtuOverTcpService modbusService, ILogger logger)
        {
            _modbusService = modbusService ?? throw new ArgumentNullException(nameof(modbusService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ConnectionResult> ConnectAsync(string ipAddress, int port)
        {
            try
            {
                _logger.Info($"Conectando {ipAddress}");

                for (byte unitId = 1; unitId <= 3; unitId++)
                {
                    try
                    {
                        await _modbusService.ConnectAsync(ipAddress, port, unitId);

                        string testResult = await _modbusService.TestConnectionAsync();

                        if (testResult.Contains("OK"))
                        {
                            // Lê versão do firmware
                            float firmwareVersion = 0.0f;
                            try
                            {
                                firmwareVersion = await _modbusService.ReadFloatAsync(ModbusRtuOverTcpService.ADDR_VERSION);
                                _logger.Info($"Equipamento conectado - Firmware v{firmwareVersion:F2}");
                            }
                            catch
                            {
                                _logger.Info("Equipamento conectado");
                            }

                            OnConnectionStatusChanged(true, "Conectado", firmwareVersion);
                            return ConnectionResult.CreateSuccess("Conectado", firmwareVersion);
                        }

                        _modbusService.Disconnect();
                    }
                    catch (Exception ex)
                    {
                        _modbusService.Disconnect();
                    }
                }

                var failureMessage = "Falha na conexão";
                OnConnectionStatusChanged(false, failureMessage, 0.0f);
                return ConnectionResult.CreateFailure(failureMessage);
            }
            catch (Exception ex)
            {
                var errorMessage = $"Erro na conexão: {ex.Message}";
                _logger.Error(errorMessage, ex);
                OnConnectionStatusChanged(false, errorMessage, 0.0f);
                return ConnectionResult.CreateFailure(errorMessage, ex);
            }
        }

        public async Task DisconnectAsync()
        {
            try
            {
                await Task.Run(() => _modbusService?.Disconnect());
                OnConnectionStatusChanged(false, "Desconectado", 0.0f);
                _logger.Info("Equipamento desconectado");
            }
            catch (Exception ex)
            {
                _logger.Error($"Erro ao desconectar: {ex.Message}", ex);
                throw;
            }
        }

        private void OnConnectionStatusChanged(bool isConnected, string message, float firmwareVersion)
        {
            ConnectionStatusChanged?.Invoke(this, new ConnectionStatusEventArgs
            {
                IsConnected = isConnected,
                Message = message,
                FirmwareVersion = firmwareVersion
            });
        }
    }

    // ===== DATA READER =====
    public class MeasurementData
    {
        
        public float VoltageA { get; set; }
        public float VoltageB { get; set; }
        public float VoltageC { get; set; }
        public float VoltageN { get; set; }

        // --- NOVAS PROPRIEDADES DE CORRENTE ---
        public float CurrentA { get; set; }
        public float CurrentB { get; set; }
        public float CurrentC { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.Now;

        public bool HasValidVoltages => VoltageA > 0 || VoltageB > 0 || VoltageC > 0;

        // Helpers de exibição
        public string VoltageDisplayA => $"{VoltageA:F1} V";
        public string VoltageDisplayB => $"{VoltageB:F1} V";
        public string VoltageDisplayC => $"{VoltageC:F1} V";
        public string VoltageDisplayN => $"{VoltageN:F1} V";

        // ---
        public string CurrentDisplayA => $"{CurrentA:F2} A";
        public string CurrentDisplayB => $"{CurrentB:F2} A";
        public string CurrentDisplayC => $"{CurrentC:F2} A";
    }

    public interface IDataReader
    {
        Task<MeasurementData> ReadMeasurementsAsync();
    }

    public class ModbusDataReader : IDataReader
    {
        private readonly ModbusRtuOverTcpService _modbusService;
        private readonly ILogger _logger;

        public ModbusDataReader(ModbusRtuOverTcpService modbusService, ILogger logger)
        {
            _modbusService = modbusService ?? throw new ArgumentNullException(nameof(modbusService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<MeasurementData> ReadMeasurementsAsync()
        {
            var data = new MeasurementData();

            try
            {
                // --- LEITURA DE TENSÕES ---
                data.VoltageA = await _modbusService.ReadFloatAsync(ModbusRtuOverTcpService.ADDR_URMS_AN);
                await Task.Delay(30); // Delay reduzido após estabilização do protocolo

                data.VoltageB = await _modbusService.ReadFloatAsync(ModbusRtuOverTcpService.ADDR_URMS_BN);
                await Task.Delay(30);

                data.VoltageC = await _modbusService.ReadFloatAsync(ModbusRtuOverTcpService.ADDR_URMS_CN);
                await Task.Delay(30);

                // --- LEITURA DE CORRENTES ---
                data.CurrentA = await _modbusService.ReadFloatAsync(ModbusRtuOverTcpService.ADDR_IRMS_A);
                await Task.Delay(30);

                data.CurrentB = await _modbusService.ReadFloatAsync(ModbusRtuOverTcpService.ADDR_IRMS_B);
                await Task.Delay(30);

                data.CurrentC = await _modbusService.ReadFloatAsync(ModbusRtuOverTcpService.ADDR_IRMS_C);
                await Task.Delay(30);

                return data;
            }
            catch (Exception ex)
            {
                _logger.Warning($"Erro leitura completa: {ex.Message}");
                throw;
            }
        }
    }

    // ===== UI STATE MANAGER =====
    public enum UiState
    {
        Disconnected,
        Connecting,
        Connected
    }

    public interface IUiStateManager
    {
        void SetConnecting();
        void SetConnected(float firmwareVersion = 0.0f);
        void SetDisconnected();
        void SetIdle();
        void UpdateConnectionStatus(bool isConnected, string message, float firmwareVersion = 0.0f);
        void UpdateMeasurementDisplay(MeasurementData measurements);
        void ShowError(string message);
        void ShowSuccess(string message);
        void ShowWarning(string message);


    }

    public class UiStateManager : IUiStateManager
    {
        private readonly Form1 _form;
        private readonly ILogger _logger;

        public UiStateManager(Form1 form, ILogger logger)
        {
            _form = form ?? throw new ArgumentNullException(nameof(form));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void SetConnecting()
        {
            InvokeOnUiThread(() =>
            {
                _form.BtnConectar.Enabled = false;
                _form.BtnDesconectar.Enabled = false;
                _form.BtnCalibrarEscala.Enabled = false;
                _form.LblTensaoDut.Text = "Conectando...";
                _form.LblTensaoDutVB.Text = "";
                _form.LblTensaoDutVC.Text = "";
                _form.LblTensaoDutVN.Text = "";
            });
        }

        public void SetConnected(float firmwareVersion = 0.0f)
        {
            InvokeOnUiThread(() =>
            {
                _form.BtnConectar.Enabled = false;
                _form.BtnDesconectar.Enabled = true;
                _form.BtnCalibrarEscala.Enabled = true;
                _form.LblTensaoDut.Text = "Lendo dados...";
            });
        }

        public void SetDisconnected()
        {
            InvokeOnUiThread(() =>
            {
                _form.BtnConectar.Enabled = true;
                _form.BtnDesconectar.Enabled = false;
                _form.BtnCalibrarEscala.Enabled = false;
                _form.LblTensaoDut.Text = "00.0 V";
                _form.LblTensaoDutVB.Text = "00.0 V";
                _form.LblTensaoDutVC.Text = "00.0 V";
                _form.LblTensaoDutVN.Text = "00.0 V";
            });
        }

        public void SetIdle()
        {
            InvokeOnUiThread(() => _form.BtnConectar.Enabled = true);
        }

        public void UpdateConnectionStatus(bool isConnected, string message, float firmwareVersion = 0.0f)
        {
            if (isConnected)
                SetConnected(firmwareVersion);
            else
                SetDisconnected();
        }

        public void UpdateMeasurementDisplay(MeasurementData measurements)
        {
            InvokeOnUiThread(() =>
            {
                // Tensões
                _form.LblTensaoDut.Text = measurements.VoltageDisplayA;
                _form.LblTensaoDutVB.Text = measurements.VoltageDisplayB;
                _form.LblTensaoDutVC.Text = measurements.VoltageDisplayC;
                _form.LblTensaoDutVN.Text = measurements.VoltageDisplayN;

                // Correntes (Usando as propriedades públicas que você criou no Form1)
                _form.LblCorrenteDut.Text = measurements.CurrentDisplayA;
                _form.LblCorrenteDutB.Text = measurements.CurrentDisplayB;
                _form.LblCorrenteDutC.Text = measurements.CurrentDisplayC;
                _form.LblCorrenteDutN.Text = "0.00 A"; // Caso precise do Neutro futuramente
            });
        }

        public void ShowError(string message)
        {
            InvokeOnUiThread(() => MessageBox.Show(message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error));
        }

        public void ShowSuccess(string message)
        {
            InvokeOnUiThread(() => MessageBox.Show(message, "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information));
        }

        public void ShowWarning(string message)
        {
            InvokeOnUiThread(() => MessageBox.Show(message, "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning));
        }

        private void InvokeOnUiThread(Action action)
        {
            if (_form.InvokeRequired)
                _form.Invoke(action);
            else
                action();
        }
    }

    // ===== LOG MANAGER LIMPO =====
    public class LogManager
    {
        private readonly RichTextBox _logTextBox;

        public LogManager(RichTextBox logTextBox)
        {
            _logTextBox = logTextBox ?? throw new ArgumentNullException(nameof(logTextBox));
        }

        public void AppendLog(LogEventArgs logEntry)
        {
            if (_logTextBox.InvokeRequired)
            {
                _logTextBox.Invoke(new Action<LogEventArgs>(AppendLog), logEntry);
                return;
            }

            try
            {
                string dayOfWeek = logEntry.Timestamp.ToString("ddd", new CultureInfo("pt-BR"));
                string dateTime = logEntry.Timestamp.ToString("dd/MM HH:mm:ss");
                string message = $"[{dayOfWeek} {dateTime}] {logEntry.Message}";

                _logTextBox.AppendText(message + Environment.NewLine);

                // Limita linhas
                if (_logTextBox.Lines.Length > 100)
                {
                    var lines = _logTextBox.Lines.Skip(50).ToArray();
                    _logTextBox.Lines = lines;
                }

                // Auto scroll
                _logTextBox.SelectionStart = _logTextBox.Text.Length;
                _logTextBox.ScrollToCaret();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro no log: {ex.Message}");
            }
        }
    }
}