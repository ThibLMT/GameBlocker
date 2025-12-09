// 1. Imports
// 'useState' is the Hook for local state (like 'let count = 0' in Svelte)
// 'useEffect' is for side-effects (like 'onMount' in Svelte)
import { useState, useEffect } from 'react'

// TypeScript Interface (like a Go struct for JSON)
interface LogEntry {
    id: number;
    message: string;
    timestamp: string;
}

function App() {
    // 2. State Definitions
    // Syntax: const [variable, setter] = useState(default);
    const [isEnabled, setIsEnabled] = useState<boolean>(true);
    const [killCount, setKillCount] = useState<number>(0);
    const [logs, setLogs] = useState<LogEntry[]>([]);
    const [newProcess, setNewProcess] = useState<string>(""); // Input binding

    // 3. Mock Data Loading (Simulating an API call)
    // useEffect(fn, []) -> Runs ONCE when component mounts
    useEffect(() => {
        // Add a fake log entry
        setLogs([
            { id: 1, message: "System initialized", timestamp: new Date().toLocaleTimeString() },
            { id: 2, message: "Monitoring bf6...", timestamp: new Date().toLocaleTimeString() }
        ]);
    }, []);

    // 4. Event Handlers
    const toggleService = () => {
        // In React, you use the setter, you don't do isEnabled = !isEnabled
        setIsEnabled(prev => !prev);

        // Optimistic UI update (simulate log)
        const action = !isEnabled ? "Resumed" : "Paused";
        addLog(`Service ${action} by user`);
    };

    const handleSimulateKill = () => {
        setKillCount(c => c + 1);
        addLog("VIOLATION: Killed 'notepad.exe'");
    };

    const addLog = (msg: string) => {
        const newLog: LogEntry = {
            id: Date.now(),
            message: msg,
            timestamp: new Date().toLocaleTimeString()
        };
        // Spread operator (...) to append to array.
        // Like Go: logs = append([]LogEntry{newLog}, logs...)
        setLogs(prevLogs => [newLog, ...prevLogs]);
    };

    // 5. Rendering (JSX)
    // Looks like HTML, but it's JavaScript.
    // - class -> className
    // - onclick -> onClick
    return (
        <div className="min-h-screen p-8 font-sans selection:bg-blue-500">
            <div className="max-w-4xl mx-auto space-y-6">

                {/* HEADER */}
                <header className="flex items-center justify-between border-b border-gray-700 pb-6">
                    <div>
                        <h1 className="text-4xl font-extrabold bg-gradient-to-r from-blue-400 to-purple-500 bg-clip-text text-transparent">
                            GameBlocker
                        </h1>
                        <p className="text-gray-400 mt-1">Daemon Status: <span className="text-mono">v1.0.0</span></p>
                    </div>

                    <div className={`px-4 py-2 rounded-full border ${isEnabled ? 'border-green-500/30 bg-green-500/10 text-green-400' : 'border-red-500/30 bg-red-500/10 text-red-400'}`}>
                        <span className="animate-pulse mr-2">●</span>
                        {isEnabled ? 'Monitoring Active' : 'Service Paused'}
                    </div>
                </header>

                {/* CONTROLS GRID */}
                <div className="grid grid-cols-1 md:grid-cols-2 gap-6">

                    {/* Main Control Card */}
                    <div className="bg-gray-800 rounded-xl p-6 border border-gray-700 shadow-xl">
                        <h2 className="text-xl font-bold mb-4 text-gray-200">Control Panel</h2>
                        <div className="space-y-4">
                            <button
                                onClick={toggleService}
                                className={`w-full py-4 rounded-lg font-bold text-lg transition-all transform hover:scale-[1.02] active:scale-[0.98] ${
                                    isEnabled
                                        ? 'bg-gradient-to-r from-red-500 to-red-600 shadow-red-900/20 shadow-lg'
                                        : 'bg-gradient-to-r from-green-500 to-green-600 shadow-green-900/20 shadow-lg'
                                }`}
                            >
                                {isEnabled ? '🛑 STOP MONITORING' : '▶ START MONITORING'}
                            </button>

                            <div className="flex gap-2">
                                <button
                                    onClick={handleSimulateKill}
                                    className="flex-1 bg-gray-700 hover:bg-gray-600 py-2 rounded-lg text-sm border border-gray-600 transition-colors"
                                >
                                    🔫 Simulate Kill
                                </button>
                            </div>
                        </div>
                    </div>

                    {/* Stats Card */}
                    <div className="bg-gray-800 rounded-xl p-6 border border-gray-700 shadow-xl flex flex-col justify-center items-center">
                        <h2 className="text-gray-400 text-sm uppercase tracking-wider font-semibold">Processes Terminated</h2>
                        <div className="text-7xl font-mono font-bold mt-2 text-blue-400 drop-shadow-lg">
                            {killCount}
                        </div>
                        <p className="text-gray-500 text-xs mt-4">Session Total</p>
                    </div>
                </div>

                {/* LOGS PANEL */}
                <div className="bg-black/30 rounded-xl border border-gray-700 overflow-hidden">
                    <div className="bg-gray-800/50 px-6 py-3 border-b border-gray-700 flex justify-between items-center">
                        <h3 className="font-mono text-sm text-gray-400">Activity Log</h3>
                        <button onClick={() => setLogs([])} className="text-xs text-gray-500 hover:text-white">Clear</button>
                    </div>
                    <div className="h-64 overflow-y-auto p-4 font-mono text-sm space-y-2">
                        {logs.length === 0 && <div className="text-gray-600 text-center italic mt-10">No activity recorded.</div>}

                        {/* React Loop: .map() instead of {#each} */}
                        {logs.map((log) => (
                            <div key={log.id} className="flex gap-3 border-l-2 border-transparent hover:border-blue-500 pl-2 transition-all">
                                <span className="text-gray-500">[{log.timestamp}]</span>
                                <span className={log.message.includes("VIOLATION") ? "text-red-400" : "text-gray-300"}>
                  {log.message}
                </span>
                            </div>
                        ))}
                    </div>
                </div>

            </div>
        </div>
    )
}

export default App