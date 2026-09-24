import type { ReactNode } from "react";

/** Every screen: title left, its one toolbar right, then content. */
export function Page({ title, toolbar, children }: { title: string; toolbar?: ReactNode; children: ReactNode })
{
	return (
		<section className="page">
			<header className="page-header">
				<h2>{title}</h2>
				{toolbar && <div className="toolbar">{toolbar}</div>}
			</header>
			{children}
		</section>
	);
}
