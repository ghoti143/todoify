import { useState } from "react";
import { LoginForm } from "./components/Auth/LoginForm";
import { RegisterForm } from "./components/Auth/RegisterForm";
import { TaskList } from "./components/Tasks/TaskList";
import { useAuth } from "./context/AuthContext";
import { AuthProvider } from "./context/AuthProvider";

type AuthView = "login" | "register";

const AppShell = () => {
  const { user, logout } = useAuth();
  const [view, setView] = useState<AuthView>("login");

  if (user) {
    return (
      <div className="min-h-screen bg-[#080a0f] text-slate-100">
        <header className="border-b border-slate-800 px-6 py-3 flex items-center justify-between">
          <span className="text-amber-400 font-mono text-sm tracking-widest uppercase">
            ◈ Tasks
          </span>
          <div className="flex items-center gap-3">
            <span className="text-slate-500 font-mono text-xs">
              {user.email}
            </span>
            <button
              onClick={logout}
              className="font-mono text-xs text-slate-500 hover:text-slate-200 border border-slate-700 hover:border-slate-500 px-3 py-1.5 rounded transition-colors"
            >
              Sign out
            </button>
          </div>
        </header>
        <main className="px-4 py-8">
          <TaskList />
        </main>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-[#080a0f] flex flex-col items-center justify-center px-4">
      <div className="mb-8 text-center">
        <span className="text-amber-400 text-4xl">◈</span>
        <h1 className="mt-3 font-mono text-2xl font-semibold tracking-tight text-slate-100">
          Todoify
        </h1>
        <p className="mt-1 font-mono text-xs text-slate-500 uppercase tracking-widest">
          Stay on top of things
        </p>
      </div>

      {view === "login" ? (
        <LoginForm onSwitch={() => setView("register")} />
      ) : (
        <RegisterForm onSwitch={() => setView("login")} />
      )}
    </div>
  );
};

const App = () => (
  <AuthProvider>
    <AppShell />
  </AuthProvider>
);

export default App;
