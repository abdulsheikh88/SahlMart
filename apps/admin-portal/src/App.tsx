// TODO: wrap with MsalProvider (Microsoft Entra ID, authorization code + PKCE) and add routes
// for Dashboard, Catalog, Stores, Inventory, Orders, Offers, Users and Roles, Audit.
export default function App() {
  return (
    <main style={{ fontFamily: "system-ui, sans-serif", padding: 24 }}>
      <h1>SHALMART Admin</h1>
      <p>Admin portal scaffold. Sign-in through Microsoft Entra ID will be wired here.</p>
    </main>
  );
}
