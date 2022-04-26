import LoginForm from "./login.jsx";
import "./App.css";

function App() {
  const title = <h1>璀璨宝石</h1>;

  return (
    <div className="App">
      <header className="App-header">
        {title}
        <LoginForm />
        <nav
          style={{
            borderBottom: "solid 1px",
            paddingBottom: "1rem",
          }}
        ></nav>
      </header>
    </div>
  );
}

export default App;
