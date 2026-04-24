import { useState } from "react";
import { useAuth } from "../../context/AuthContext";

interface LoginFormProps {
  onSwitch: () => void;
}

export const LoginForm = ({ onSwitch }: LoginFormProps) => {
  const { login } = useAuth();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setLoading(true);
    try {
      await login(email, password);
    } catch (err: unknown) {
      setError(
        err instanceof Error
          ? err.message
          : "Login failed. Check your credentials.",
      );
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="w-full max-w-sm bg-[#0f1117] border border-slate-800 rounded p-8 font-mono">
      {error && (
        <div className="mb-5 px-4 py-3 bg-red-950/50 border border-red-800/50 rounded text-red-300 text-xs leading-relaxed">
          {error}
        </div>
      )}

      <form onSubmit={handleSubmit} className="flex flex-col gap-5">
        <label className="flex flex-col gap-1.5">
          <span className="text-slate-500 text-xs uppercase tracking-widest">
            Email
          </span>
          <input
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            placeholder="you@example.com"
            required
            autoFocus
            className="bg-[#1a1d27] border border-slate-700 rounded px-3 py-2.5 text-slate-100 text-sm placeholder-slate-600 outline-none focus:border-amber-400 transition-colors"
          />
        </label>

        <label className="flex flex-col gap-1.5">
          <span className="text-slate-500 text-xs uppercase tracking-widest">
            Password
          </span>
          <input
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            placeholder="••••••••"
            required
            className="bg-[#1a1d27] border border-slate-700 rounded px-3 py-2.5 text-slate-100 text-sm placeholder-slate-600 outline-none focus:border-amber-400 transition-colors"
          />
        </label>

        <button
          type="submit"
          disabled={loading}
          data-testid="submit-button"
          className="mt-1 py-2.5 bg-amber-400 hover:bg-amber-300 disabled:opacity-50 disabled:cursor-not-allowed text-slate-900 text-xs font-bold uppercase tracking-widest rounded transition-colors flex items-center justify-center min-h-[42px]"
        >
          {loading ? (
            <span className="w-4 h-4 border-2 border-slate-900/30 border-t-slate-900 rounded-full animate-spin" />
          ) : (
            "Sign in"
          )}
        </button>
      </form>

      <p className="mt-6 text-center text-slate-600 text-xs">
        No account?{" "}
        <button
          onClick={onSwitch}
          className="text-amber-400 hover:text-amber-300 underline underline-offset-2 transition-colors"
        >
          Register
        </button>
      </p>
    </div>
  );
};
