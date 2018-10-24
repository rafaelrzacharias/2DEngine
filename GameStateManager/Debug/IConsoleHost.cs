using System.Collections.Generic;

namespace GameStateManager
{
    // Message types for the console.
    public enum ConsoleMessage
    {
        STANDARD = 1,
        ERROR = 2,
        WARNING = 3
    }

    // Console execution delegate.
    public delegate void ConsoleExecute(IConsoleHost host, string command, List<string> arguments);

    // Interface for console executioner.
    public interface IConsoleExecutioner
    {
        void ExecuteCommand(string command);
    }

    // Interface for console message listener.
    public interface IConsoleEchoListener
    {
        void Echo(ConsoleMessage messageType, string text);
    }


    // Interface for console host.
    public interface IConsoleHost : IConsoleEchoListener, IConsoleExecutioner
    {
        // Register a new command.
        void RegisterCommand(string command, string description, ConsoleExecute callback);

        // Unregister a command.
        void UnregisterCommand(string command);

        // Output Standard message.
        void Echo(string text);

        // Output Warning message.
        void EchoWarning(string text);

        // Output Error message.
        void EchoError(string text);

        // Register the message listener.
        void RegisterEchoListener(IConsoleEchoListener listener);

        // Unregister the message listener.
        void UnregisterEchoListener(IConsoleEchoListener listener);

        // Add Command executioner.
        void PushExecutioner(IConsoleExecutioner executioner);
    }
}