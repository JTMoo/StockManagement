import { useEffect, useState } from "react";
import type { ApiFailure, Result } from "./api";

/** Loads once on mount; `setData` keeps local changes (e.g. a created row) without reloading. */
export function useLoad<T>(load: (signal: AbortSignal) => Promise<Result<T>>)
{
	const [data, setData] = useState<T>();
	const [failure, setFailure] = useState<ApiFailure>();

	useEffect(() =>
	{
		const controller = new AbortController();
		load(controller.signal).then(result =>
		{
			if (controller.signal.aborted) return;
			if (result.ok) setData(result.value);
			else setFailure(result.failure);
		});
		return () => controller.abort();
	}, [load]);

	return { data, setData, failure };
}
