const API_URL = "http://localhost:5000/api";

export interface StatusResponse {
    isEnabled: boolean;
    blockedCount: number;
    recentLogs: LogEntry[];
}

export interface  LogEntry {
    id: number;
    message: string;
    timestamp: string;
}

export interface GameScanResult {
    name: string;
    exes: string[];
}

export const getStatus = async (): Promise<StatusResponse> => {
    try {
        const res = await fetch(`${API_URL}/status`);
        if (!res.ok) throw new Error("Network response was not ok");
        return await res.json();
    } catch (error) {
        console.error("API Error:", error);
        // Return fallback data if server is down
        return { isEnabled: false, blockedCount: 0, recentLogs: [] };
    }
};

export const toggleService = async (): Promise<void> => {
    await fetch(`${API_URL}/toggle`, { method: "POST" });
};

export const scanFolder = async (path: string): Promise<GameScanResult[]> => {
    const params = new URLSearchParams({path: path});
    try {
        const res = await fetch(`${API_URL}/scan?${params}`, {method: "POST"});
        return await res.json();
    } catch (error) {
        console.error("API Error:", error);
        return  [];
    }
}