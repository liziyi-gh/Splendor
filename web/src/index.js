import { render } from "react-dom";
import { BrowserRouter, Routes, Route } from "react-router-dom";
import App from "./App";
import "./index.css";
import { GamePage } from "./GamePage.jsx";

const rootElement = document.getElementById("root");
render(
  <BrowserRouter>
    <Routes>
      <Route path="/" element={<App />} />
      <Route path="gamepage" element={<GamePage />} />
    </Routes>
  </BrowserRouter>,
  rootElement
);
