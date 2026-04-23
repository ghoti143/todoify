import { useState } from "react";
import type { AuthResponse } from "../types";
import { api } from "../api/client";
import { AuthContext } from "./AuthContext";

export const AuthProvider = ({ children }: { children: React.ReactNode }) => {
  const [user, setUser] = useState<AuthResponse | null>(() => {
    const token = localStorage.getItem("token");
    const email = localStorage.getItem("email");
    return token && email ? ({ token, email } as AuthResponse) : null;
  });

  const login = async (email: string, password: string) => {
    const res = await api.post<AuthResponse>("/auth/login", {
      email,
      password,
    });
    localStorage.setItem("token", res.data.token);
    localStorage.setItem("email", res.data.email);
    setUser(res.data);
  };

  const register = async (
    email: string,
    password: string,
    displayName?: string,
  ) => {
    const res = await api.post<AuthResponse>("/auth/register", {
      email,
      password,
      displayName,
    });
    localStorage.setItem("token", res.data.token);
    localStorage.setItem("email", res.data.email);
    setUser(res.data);
  };

  const logout = () => {
    localStorage.removeItem("token");
    localStorage.removeItem("email");
    setUser(null);
  };

  return (
    <AuthContext.Provider value={{ user, login, register, logout }}>
      {children}
    </AuthContext.Provider>
  );
};
