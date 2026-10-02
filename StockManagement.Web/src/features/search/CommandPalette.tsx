import { Search } from "lucide-react";
import { useEffect, useMemo, useRef, useState } from "react";
import { api, type SearchDomain, type SearchGroup } from "../../api";
import { useI18n } from "../../i18n";
import type { NavRoute, View } from "../../routes";

const domainToView: Record<SearchDomain, View> = {
	StockItems: "stockItems",
	Customers: "clients",
	Invoices: "invoices",
	Suppliers: "suppliers"
};

type Props = {
	routes: NavRoute[];
	onNavigate: (view: View) => void;
	onClose: () => void;
};

export function CommandPalette({ routes, onNavigate, onClose }: Props)
{
	const { t } = useI18n();
	const [query, setQuery] = useState("");
	const [includeInactive, setIncludeInactive] = useState(false);
	const [groups, setGroups] = useState<SearchGroup[]>([]);
	const [selected, setSelected] = useState(0);
	const inputRef = useRef<HTMLInputElement>(null);

	useEffect(() => { inputRef.current?.focus(); }, []);

	useEffect(() =>
	{
		setSelected(0);
		if (!query.trim())
		{
			setGroups([]);
			return;
		}

		const controller = new AbortController();
		const timeout = setTimeout(async () =>
		{
			const result = await api.search(query.trim(), includeInactive, controller.signal);
			if (result.ok) setGroups(result.value.groups);
		}, 200);

		return () =>
		{
			clearTimeout(timeout);
			controller.abort();
		};
	}, [query, includeInactive]);

	const screenMatches = useMemo(
		() => (query.trim() ? routes.filter(route => t(route.name).toLowerCase().includes(query.trim().toLowerCase())) : []),
		[query, routes, t]);

	// Flattened in display order, so ArrowUp/Down + Enter work across screens and every domain's hits
	const flat = useMemo(() =>
	{
		const list: Array<{ activate: () => void }> = [];
		for (const route of screenMatches) list.push({ activate: () => onNavigate(route.name) });
		for (const group of groups) for (const _hit of group.items) list.push({ activate: () => onNavigate(domainToView[group.domain]) });
		return list;
	}, [screenMatches, groups, onNavigate]);

	function onKeyDown(event: React.KeyboardEvent)
	{
		if (event.key === "Escape") { onClose(); return; }
		if (event.key === "ArrowDown") { event.preventDefault(); setSelected(index => Math.min(index + 1, flat.length - 1)); return; }
		if (event.key === "ArrowUp") { event.preventDefault(); setSelected(index => Math.max(index - 1, 0)); return; }
		if (event.key === "Enter")
		{
			event.preventDefault();
			const entry = flat[selected];
			if (entry) { entry.activate(); onClose(); }
		}
	}

	let rowIndex = -1;

	return (
		<div className="scrim" onClick={onClose}>
			<div className="palette" role="dialog" aria-modal="true" onClick={event => event.stopPropagation()} onKeyDown={onKeyDown}>
				<div className="palette-search">
					<Search size={16} />
					<input ref={inputRef} value={query} onChange={event => setQuery(event.target.value)} placeholder={t("searchBoxDefault")} aria-label={t("search")} />
					<button type="button" className="palette-filter" aria-pressed={includeInactive} onClick={() => setIncludeInactive(!includeInactive)}>
						{t("searchIncludeInactive")}
					</button>
				</div>

				{screenMatches.length > 0 && (
					<div className="palette-section">
						<div className="palette-section-title">{t("searchScreens")}</div>
						{screenMatches.map(route =>
						{
							rowIndex++;
							const index = rowIndex;
							return (
								<button key={route.name} type="button" className="palette-result" aria-selected={selected === index}
									onClick={() => { onNavigate(route.name); onClose(); }}>
									{t(route.name)}
								</button>
							);
						})}
					</div>
				)}

				{groups.filter(group => group.items.length > 0).map(group => (
					<div className="palette-section" key={group.domain}>
						<div className="palette-section-title">
							<span>{t(domainToView[group.domain])}</span>
							<span>{group.totalCount}</span>
						</div>
						{group.items.map(hit =>
						{
							rowIndex++;
							const index = rowIndex;
							return (
								<button key={hit.id} type="button" className="palette-result" aria-selected={selected === index}
									onClick={() => { onNavigate(domainToView[group.domain]); onClose(); }}>
									{hit.title}
									<span className="palette-meta">{hit.subtitle}</span>
								</button>
							);
						})}
						{group.totalCount > group.items.length && (
							<button type="button" className="palette-more" onClick={() => { onNavigate(domainToView[group.domain]); onClose(); }}>
								{t("searchViewAllResults").replace("{0}", String(group.totalCount)).replace("{1}", t(domainToView[group.domain]))}
							</button>
						)}
					</div>
				))}

				{query.trim() && screenMatches.length === 0 && groups.every(group => group.items.length === 0) && (
					<div className="palette-empty">{t("searchNoResults")}</div>
				)}
			</div>
		</div>
	);
}
