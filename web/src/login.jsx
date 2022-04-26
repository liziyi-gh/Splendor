import React from "react";
import axios from "axios";
import { useNavigate } from "react-router-dom";
import "./login.css";

class __LoginForm extends React.Component {
  constructor(props) {
    super(props);
    this.state = { username: "", password: "" };

    this.handleChange = this.handleChange.bind(this);
    this.handleSubmit = this.handleSubmit.bind(this);
    // this.loginUrl = "/api/login.json";
    this.loginUrl = "http://127.0.0.1:8000/login/";
  }

  handleChange(event) {
    const target = event.target;
    const input_type = target.name;
    this.setState({ [input_type]: event.target.value });
  }

  handleSubmit(event) {
    axios.post(this.loginUrl).then(
      (res) => {
        const loginFeedback = res.data["loginFeedback"];
        if (loginFeedback === true) {
          this.props.navigate("gamepage", { replace: true });
        }
      },
      (error) => {
        if (error.response) {
          // The request was made and the server responded with a status code
          // that falls out of the range of 2xx
          console.log(error.response.data);
          console.log(error.response.status);
          console.log(error.response.headers);
        } else if (error.request) {
          // The request was made but no response was received
          // `error.request` is an instance of XMLHttpRequest in the browser and an instance of
          // http.ClientRequest in node.js
          console.log(error.request);
        } else {
          // Something happened in setting up the request that triggered an Error
          console.log("Error", error.message);
        }
        console.log(error.config);
      }
    );
    event.preventDefault();
  }

  render() {
    return (
      <form onSubmit={this.handleSubmit}>
        <label>
          用户名:
          <input
            name="username"
            type="text"
            value={this.state.username}
            onChange={this.handleChange}
          />
          <br />
          密码:
          <input
            name="password"
            type="password"
            value={this.state.password}
            onChange={this.handleChange}
          />
          <br />
          <input className="Login-button-blue" type="submit" value="sign-in" />
        </label>
      </form>
    );
  }
}

function LoginForm(props) {
  let navigate = useNavigate();

  return <__LoginForm {...props} navigate={navigate} />;
}

export default LoginForm;
