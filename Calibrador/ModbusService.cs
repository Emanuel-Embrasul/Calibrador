// ModbusRtuOverTcpService.cs - Serviço completo para Modbus RTU sobre TCP
using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace Calibrador
{
    public interface IModbusRtuService : IDisposable
    {
        bool IsConnected { get; }
        Task ConnectAsync(string ipAddress, int port, byte unitId);
        void Disconnect();
        Task<float> ReadFloatAsync(ushort address);
        Task<ushort[]> ReadHoldingRegistersAsync(ushort address, ushort count);
        Task<string> TestConnectionAsync();
    }

    public class ModbusRtuOverTcpService : IModbusRtuService
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private byte _unitId;

        // Endereços corretos baseados no código fonte do equipamento
        public const ushort ADDR_VERSION = 0;        // Version (0-1)
        public const ushort ADDR_URMS_AN = 68;       // UrmsAN (68-69)
        public const ushort ADDR_URMS_BN = 70;       // UrmsBN (70-71) 
        public const ushort ADDR_URMS_CN = 72;       // UrmsCN (72-73)
        public const ushort ADDR_IRMS_A = 74;        // IrmsA (74-75)
        public const ushort ADDR_IRMS_B = 76;        // IrmsB (76-77)
        public const ushort ADDR_IRMS_C = 78;        // IrmsC (78-79)
        public const ushort ADDR_FREQ_A = 66;        // FreqA (66-67)

        public bool IsConnected => _client?.Connected ?? false;

        public async Task ConnectAsync(string ipAddress, int port, byte unitId)
        {
            try
            {
                if (IsConnected) Disconnect();

                _client = new TcpClient();
                _client.ReceiveTimeout = 5000;
                _client.SendTimeout = 5000;

                var connectTask = _client.ConnectAsync(ipAddress, port);
                if (await Task.WhenAny(connectTask, Task.Delay(10000)) == connectTask)
                {
                    await connectTask;
                }
                else
                {
                    throw new TimeoutException("Timeout na conexão TCP");
                }

                if (!_client.Connected)
                {
                    throw new Exception("Falha ao conectar com o equipamento.");
                }

                _stream = _client.GetStream();
                _unitId = unitId;

                System.Diagnostics.Debug.WriteLine($"Conectado RTU over TCP: {ipAddress}:{port}, Unit ID: {unitId}");
            }
            catch (Exception ex)
            {
                Disconnect();
                throw new Exception($"Erro ao conectar: {ex.Message}", ex);
            }
        }

        public async Task<string> TestConnectionAsync()
        {
            try
            {
                if (!IsConnected)
                    return "❌ Não conectado";

                // Testa leitura de um registro básico
                var response = await SendModbusRtuCommand(_unitId, 0x03, 0, 1);
                if (response != null && response.Length > 0)
                {
                    return $"✅ Resposta OK com Unit ID {_unitId}: {BytesToHex(response)}";
                }

                return "❌ Sem resposta";
            }
            catch (Exception ex)
            {
                return $"❌ Erro no teste: {ex.Message}";
            }
        }

        public async Task<float> ReadFloatAsync(ushort address)
        {
            if (!IsConnected)
                throw new InvalidOperationException("Cliente não está conectado.");

            try
            {
                // Lê 2 registros para formar um float de 32 bits
                var response = await SendModbusRtuCommand(_unitId, 0x03, address, 2);

                if (response == null || response.Length == 0)
                {
                    throw new Exception("O equipamento não respondeu (Timeout).");
                }

                // Valida CRC da resposta
                if (response.Length >= 3 && !VerifyCRC16(response))

                    if (response == null || response.Length < 7) // Min: UnitID + Func + ByteCount + 4 data bytes + 2 CRC
                    throw new Exception("Resposta muito curta ou inválida");

                // Verifica se é uma resposta de leitura válida
                if (response[1] != 0x03)
                    throw new Exception($"Function code incorreto: esperado 0x03, recebido 0x{response[1]:X2}");

                byte byteCount = response[2];
                if (byteCount != 4)
                    throw new Exception($"Byte count incorreto: esperado 4, recebido {byteCount}");

                // Extrai os 4 bytes de dados
                // Baseado no código fonte: Low word primeiro, depois High word
                byte[] floatData = new byte[4];
                floatData[0] = response[4]; // Low word LSB
                floatData[1] = response[3]; // Low word MSB  
                floatData[2] = response[6]; // High word LSB
                floatData[3] = response[5]; // High word MSB

                return BitConverter.ToSingle(floatData, 0);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao ler float no endereço {address}: {ex.Message}", ex);
            }
        }

        public async Task<ushort[]> ReadHoldingRegistersAsync(ushort address, ushort count)
        {
            if (!IsConnected)
                throw new InvalidOperationException("Cliente não está conectado.");

            try
            {
                var response = await SendModbusRtuCommand(_unitId, 0x03, address, count);

                if (response == null || response.Length < 5)
                    throw new Exception("Resposta inválida");

                if (response[1] != 0x03)
                    throw new Exception($"Function code incorreto: {response[1]:X2}");

                byte byteCount = response[2];
                int expectedBytes = count * 2;

                if (byteCount != expectedBytes)
                    throw new Exception($"Byte count incorreto: esperado {expectedBytes}, recebido {byteCount}");

                ushort[] registers = new ushort[count];
                for (int i = 0; i < count; i++)
                {
                    registers[i] = (ushort)((response[3 + i * 2] << 8) | response[4 + i * 2]);
                }

                return registers;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao ler registros no endereço {address}: {ex.Message}", ex);
            }
        }

        private async Task<byte[]> SendModbusRtuCommand(byte unitId, byte function, ushort address, ushort count)
        {
            try
            {
                // Monta comando Modbus RTU
                var command = new List<byte>();
                command.Add(unitId);
                command.Add(function);
                command.Add((byte)(address >> 8));   // Address high
                command.Add((byte)(address & 0xFF)); // Address low
                command.Add((byte)(count >> 8));     // Count high
                command.Add((byte)(count & 0xFF));   // Count low

                // Calcula CRC16
                ushort crc = CalculateCRC16(command.ToArray());
                command.Add((byte)(crc & 0xFF));        // CRC low
                command.Add((byte)((crc >> 8) & 0xFF)); // CRC high

                System.Diagnostics.Debug.WriteLine($"Enviando: {BytesToHex(command.ToArray())}");

                // Limpa buffer de recepção
                await ClearReceiveBuffer();

                // Envia comando
                await _stream.WriteAsync(command.ToArray(), 0, command.Count);

                // Lê resposta
                var response = await ReadResponseAsync(3000);

                System.Diagnostics.Debug.WriteLine($"Recebido: {BytesToHex(response)}");

                // Valida CRC da resposta
                if (response.Length >= 3 && !VerifyCRC16(response))
                {
                    throw new Exception("CRC inválido na resposta");
                }

                return response;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro no comando Modbus: {ex.Message}", ex);
            }
        }

        private async Task ClearReceiveBuffer()
        {
            while (_stream.DataAvailable)
            {
                byte[] trash = new byte[1024];
                await _stream.ReadAsync(trash, 0, trash.Length);
                await Task.Delay(10);
            }
        }

        private async Task<byte[]> ReadResponseAsync(int timeoutMs, int expectedLength = 0)
        {
            var response = new List<byte>();
            var startTime = DateTime.Now;
            byte[] buffer = new byte[256];

            while ((DateTime.Now - startTime).TotalMilliseconds < timeoutMs)
            {
                if (_stream.DataAvailable)
                {
                    int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        response.AddRange(buffer.Take(bytesRead));

                        // Se já temos o cabeçalho, podemos calcular o tamanho total esperado (RTU)
                        // Para Function 03: ID(1) + Func(1) + ByteCount(1) + Data(N) + CRC(2)
                        if (response.Count >= 3 && expectedLength == 0)
                        {
                            if (response[1] == 0x03)
                                expectedLength = response[2] + 5;
                            else if (response[1] > 0x80) // Erro Modbus
                                expectedLength = 5;
                        }

                        // Se atingimos o tamanho esperado, retornamos imediatamente
                        if (expectedLength > 0 && response.Count >= expectedLength)
                            break;
                    }
                }
                await Task.Delay(10); // Delay menor para não perder performance
            }
            return response.ToArray();
        }

        private ushort CalculateCRC16(byte[] data)
        {
            ushort crc = 0xFFFF;

            foreach (byte b in data)
            {
                crc ^= b;
                for (int i = 0; i < 8; i++)
                {
                    if ((crc & 1) != 0)
                        crc = (ushort)((crc >> 1) ^ 0xA001);
                    else
                        crc >>= 1;
                }
            }

            return crc;
        }

        private bool VerifyCRC16(byte[] data)
        {
            if (data.Length < 3) return false;

            byte[] dataWithoutCrc = data.Take(data.Length - 2).ToArray();
            ushort calculatedCrc = CalculateCRC16(dataWithoutCrc);
            ushort receivedCrc = (ushort)(data[data.Length - 2] | (data[data.Length - 1] << 8));

            return calculatedCrc == receivedCrc;
        }

        private string BytesToHex(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                return "(vazio)";
            return string.Join(" ", bytes.Select(b => b.ToString("X2")));
        }

        public void Disconnect()
        {
            try
            {
                _stream?.Close();
                _client?.Close();
                _stream?.Dispose();
                _client?.Dispose();
                _stream = null;
                _client = null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao desconectar: {ex.Message}");
            }
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}