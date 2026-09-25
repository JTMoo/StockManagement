import { createContext, useContext, useEffect, useState, type ReactNode } from "react";
import { api, setAuthToken, setUnauthorizedHandler, type Result, type LoginResult } from "./api";

const tokenKey = "auth.token";
const usernameKey = "auth.username";

type Auth = {
	username: string | null;
	login: (username: string, password: string) => Promise<Result<LoginResult>>;
	logout: () => void;
};

const AuthContext = createContext<Auth | null>(null);

function readStorage(key: string): string | null
{
	try
	{
		return localStorage.getItem(key);
	}
	catch
	{
		return null;
	}
}

function writeStorage(key: string, value: string | null)
{
	try
	{
		if (value === null) localStorage.removeItem(key);
		else localStorage.setItem(key, value);
	}
	catch
	{
		// Private window / blocked storage: session just won't survive a refresh
	}
}

export function AuthProvider({ children }: { children: ReactNode })
{
	const [username, setUsername] = useState<string | null>(() => readStorage(usernameKey));

	useEffect(() =>
	{
		const token = readStorage(tokenKey);
		if (token) setAuthToken(token);
		setUnauthorizedHandler(logout);
		return () => setUnauthorizedHandler(null);
	}, []);

	async function login(usernameInput: string, password: string): Promise<Result<LoginResult>>
	{
		const result = await api.login(usernameInput, password);
		if (result.ok)
		{
			setAuthToken(result.value.token);
			setUsername(result.value.username);
			writeStorage(tokenKey, result.value.token);
			writeStorage(usernameKey, result.value.username);
		}
		return result;
	}

	function logout()
	{
		setAuthToken(null);
		setUsername(null);
		writeStorage(tokenKey, null);
		writeStorage(usernameKey, null);
	}

	return <AuthContext.Provider value={{ username, login, logout }}>{children}</AuthContext.Provider>;
}

export function useAuth(): Auth
{
	const auth = useContext(AuthContext);
	if (!auth) throw new Error("useAuth needs an AuthProvider.");
	return auth;
}
