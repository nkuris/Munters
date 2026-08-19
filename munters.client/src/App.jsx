import { useEffect, useState } from 'react';
import './App.css';

function App() {
    const [gifs, setGifs] = useState(null);
    const [error, setError] = useState(null);
    const [query, setQuery] = useState('');
    const [loading, setLoading] = useState(false);

    useEffect(() => {
        fetchTrending();
    }, []);

    async function fetchWithFallback(path) {
        const candidates = [path, `http://host.docker.internal:8080${path}`];
        for (const url of candidates) {
            try {
                const resp = await fetch(url);
                if (!resp.ok) {
                    // try next candidate but collect message
                    const text = await resp.text().catch(() => null);
                    throw new Error(text || `${resp.status} ${resp.statusText}`);
                }
                return await resp.json();
            } catch {
                // try next
            }
        }
        throw new Error('Unable to reach backend API. Make sure the server is running and CORS is configured. Please set the Giphy API key in the server configuration. and docker-compose');
    }

    async function fetchTrending() {
        setLoading(true);
        setError(null);
        try {
            const data = await fetchWithFallback('/api/giphy/trending');
            setGifs(data);
        } catch (e) {
            setError(formatClientError(e.message ?? String(e)));
        } finally {
            setLoading(false);
        }
    }

    async function doSearch(e) {
        e?.preventDefault();
        if (!query || query.trim() === '') {
            setError('Please enter a search query.');
            return;
        }
        setLoading(true);
        setError(null);
        try {
            const encoded = encodeURIComponent(query.trim());
            const data = await fetchWithFallback(`/api/giphy/search?q=${encoded}`);
            setGifs(data);
        } catch (e) {
            setError(formatClientError(e.message ?? String(e)));
        } finally {
            setLoading(false);
        }
    }

    // Provide a clearer, actionable message to the user when the backend indicates
    // the Giphy API key is missing or authentication failed.
    function formatClientError(msg) {
        const lower = (msg || '').toLowerCase();
        if (lower.includes('giphy api key') || lower.includes('giphy:apikey') || lower.includes('giphy__apikey') || lower.includes('authentication failed') || lower.includes('unauthorized') || lower.includes('invalid api')) {
            return (
                msg +
                '\n\nThe server indicates the Giphy API key is missing or invalid. To fix this, set the key on the server:\n' +
                "- In development: run `dotnet user-secrets set \"Giphy:ApiKey\" \"<your-key>\"`\n" +
                "- Or set environment variable GIPHY__APIKEY (double underscore maps to colon in configuration)\n" +
                "- Or add to Munters.Server/appsettings.json under 'Giphy: { \"ApiKey\": \"<your-key>\" }'\n\n" +
                'Get a key at https://developers.giphy.com/'
            );
        }

        return msg;
    }

    return (
        <div>
            <h1>Giphy</h1>
            <form onSubmit={doSearch} style={{ marginBottom: 12 }}>
                <input
                    type="text"
                    placeholder="Search GIFs..."
                    value={query}
                    onChange={(ev) => setQuery(ev.target.value)}
                    style={{ padding: 8, width: 300 }}
                />
                <button type="submit" style={{ marginLeft: 8, padding: '8px 12px' }} disabled={loading}>
                    Search
                </button>
                <button type="button" style={{ marginLeft: 8, padding: '8px 12px' }} onClick={fetchTrending} disabled={loading}>
                    Trending
                </button>
            </form>

            {loading && <div>Loading...</div>}

            {error && <div style={{ color: 'red', marginBottom: 12 }}>Error: {error}</div>}

            {gifs && gifs.length === 0 && <div>No results found.</div>}

            {gifs && gifs.length > 0 && (
                <div style={{ display: 'flex', flexWrap: 'wrap', gap: 12 }}>
                    {gifs.map((g) => (
                        <div key={g.id} style={{ width: 200 }}>
                            <img src={g.previewUrl ?? g.url} alt={g.title ?? 'gif'} style={{ maxWidth: '100%' }} />
                            <div style={{ fontSize: 12 }}>{g.title}</div>
                        </div>
                    ))}
                </div>
            )}
        </div>
    );
}

export default App;
