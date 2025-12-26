import {scanFolder, type GameScanResult} from "./api.ts";
import {useState} from "react";

interface ScannerProps {
    isOpen: boolean;
    onClose: () => void;
}

export default function Scanner({ isOpen, onClose }: ScannerProps) {
    const [path, setPath] = useState<string>("A:\\Games");
    const [results, setResults] = useState<GameScanResult[]>([]);



    const handleScan = async () => {
        const data = await scanFolder(path);
        setResults(data)
    };

    if (!isOpen) return null;

    return (
        <div className="fixed inset-0 bg-black/70 backdrop-blur-sm flex items-center justify-center z-50 p-4 animate-in fade-in duration-200">
            {/* Modal Container */}
            <div className="bg-gray-800 rounded-xl border border-gray-600 shadow-2xl w-full max-w-2xl max-h-[85vh] flex flex-col">

                {/* Header */}
                <div className="flex justify-between items-center p-6 border-b border-gray-700">
                    <h2 className="text-xl font-bold text-white">Scan Game Library</h2>
                    <button
                        onClick={onClose}
                        className="text-gray-400 hover:text-white transition-colors text-2xl leading-none"
                    >
                        &times;
                    </button>
                </div>

                {/* Body (Scrollable) */}
                <div className="p-6 overflow-y-auto flex-1 space-y-6">
                    {/* Input Group */}
                    <div className="flex gap-2">
                        <input
                            value={path}
                            onChange={(e) => setPath(e.target.value)}
                            placeholder="Enter folder path (e.g. A:\Games)"
                            className="flex-1 bg-gray-900 border border-gray-600 rounded-lg p-3 text-white focus:ring-2 focus:ring-blue-500 outline-none"
                        />
                        <button
                            onClick={handleScan}
                            className="bg-blue-600 hover:bg-blue-700 text-white font-semibold px-6 rounded-lg transition-colors"
                        >
                            Scan
                        </button>
                    </div>

                    {/* Results List */}
                    {results.length > 0 && (
                        <div className="space-y-1">
                            <h3 className="text-sm font-bold text-gray-400 uppercase tracking-wider mb-2">Found {results.length} Games</h3>
                            <div className="space-y-2">
                                {results.map((game) => (
                                    <div key={game.name} className="bg-gray-700/50 p-3 rounded flex justify-between items-center hover:bg-gray-700 transition-colors">
                                        <span className="font-medium text-gray-200">{game.name}</span>
                                        <span className="text-xs text-gray-400 font-mono bg-gray-800 px-2 py-1 rounded">
                                            {game.exes.length} exe(s)
                                        </span>
                                    </div>
                                ))}
                            </div>
                        </div>
                    )}
                </div>

                {/* Footer */}
                <div className="p-4 border-t border-gray-700 bg-gray-900/50 rounded-b-xl flex justify-end gap-3">
                    <button
                        onClick={onClose}
                        className="px-4 py-2 text-gray-300 hover:text-white"
                    >
                        Cancel
                    </button>
                    <button className="bg-green-600 hover:bg-green-700 text-white px-6 py-2 rounded-lg font-bold shadow-lg shadow-green-900/20 disabled:opacity-50 disabled:cursor-not-allowed">
                        Save Selected
                    </button>
                </div>
            </div>
        </div>
    );
}