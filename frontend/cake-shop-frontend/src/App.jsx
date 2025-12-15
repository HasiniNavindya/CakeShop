import { useEffect, useState } from "react";
import { getCakes } from "./api/cakes";

function App() {
  const [cakes, setCakes] = useState([]);

  useEffect(() => {
    getCakes()
      .then(setCakes)
      .catch(console.error);
  }, []);

  return (
    <div>
      <h1>Cakes</h1>
      {cakes.map(c => (
        <div key={c.id}>
          {c.name} - ${c.price}
        </div>
      ))}
    </div>
  );
}

export default App;
