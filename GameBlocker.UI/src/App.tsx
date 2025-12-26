// 1. Imports
// 'useState' is the Hook for local state (like 'let count = 0' in Svelte)
// 'useEffect' is for side-effects (like 'onMount' in Svelte)
import { useState, useEffect } from 'react'
import {getStatus, toggleService} from "./api.ts";
import Scanner from './Scanner'

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
    const [killCount] = useState<number>(0);
    const [logs, setLogs] = useState<LogEntry[]>([]);
    const [isScannerOpen, setIsScannerOpen] = useState<boolean>(false);

    // 3. Data Loading
    // useEffect(fn, []) -> Runs ONCE when component mounts
    useEffect(() => {
        const fetchStatus = async () => {
            const data = await getStatus();
            setIsEnabled(data.isEnabled);
            setLogs(data.recentLogs);
        };
        fetchStatus();

        // Set interval (poll every 2 seconds)
        const intervalId = setInterval(fetchStatus, 2000);

        // Cleanup: Stop polling when component unmounts
        return () => clearInterval(intervalId);
    }, []);


    const handleToggle = async () => {
        // Optimistic Update (Make UI feel fast)
        setIsEnabled(!isEnabled);

        // Call API
        await toggleService();

        // Note: The next polling cycle will confirm the true state
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
                        <h1 className="text-4xl font-extrabold bg-linear-to-r from-blue-400 to-purple-500 bg-clip-text text-transparent">
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
                                onClick={handleToggle}
                                className={`w-full py-4 rounded-lg font-bold text-lg transition-all transform hover:scale-[1.02] active:scale-[0.98] ${
                                    isEnabled
                                        ? 'bg-linear-to-r from-red-500 to-red-600 shadow-red-900/20 shadow-lg'
                                        : 'bg-linear-to-r from-green-500 to-green-600 shadow-green-900/20 shadow-lg'
                                }`}
                            >
                                {isEnabled ? '🛑 STOP MONITORING' : '▶ START MONITORING'}
                            </button>
                        </div>
                        <div className="mt-4">
                            <button
                                onClick={() => setIsScannerOpen(true)}
                                className="w-full py-3 rounded-lg font-bold text-gray-300 bg-gray-700 border border-gray-600 hover:bg-gray-600 hover:text-white transition-all flex items-center justify-center gap-2"
                            >
                                ➕ Add Game Folder
                            </button>
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
                        {logs.slice().reverse().map((log) => (
                            <div key={log.id} className="flex gap-3 border-l-2 border-transparent hover:border-blue-500 pl-2 transition-all">
                                <span className="text-gray-500">[{log.timestamp}]</span>
                                <span className={log.message.includes("VIOLATION") ? "text-red-400" : "text-gray-300"}>
                  {log.message}
                </span>
                            </div>
                        ))}
                    </div>
                </div>

                <Scanner
                    isOpen={isScannerOpen}
                    onClose={() => setIsScannerOpen(false)}
                />

            </div>
        </div>
    )
}

export default App