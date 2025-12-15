const API = import.meta.env.VITE_API_URL;

export async function getCakes() {
  const res = await fetch(`${API}/Cakes`);
  if (!res.ok) throw new Error("Failed to fetch cakes");
  return res.json();
}
