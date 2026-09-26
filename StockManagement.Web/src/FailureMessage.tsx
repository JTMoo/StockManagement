import type { ApiFailure } from "./api";
import { isTextKey, useI18n, type TextKey } from "./i18n";

export function FailureMessage({ failure, notFound = "unexpectedError", unauthorized = "sessionExpired" }: { failure?: ApiFailure; notFound?: TextKey; unauthorized?: TextKey })
{
	const { t } = useI18n();
	if (!failure) return null;

	return <p role="alert" className="failure">{text()}</p>;

	function text(): string
	{
		switch (failure!.kind)
		{
			case "notFound": return t(notFound);
			case "conflict": return `${t("itemsUnavailable")} ${failure!.unavailableItems.join(", ")}`;
			case "duplicate": return `${t("itemAlreadyExists")} ${failure!.code}`;
			case "insufficientStock": return `${t("insufficientStock")} ${failure!.inStock}`;
			case "invalidState": return failure!.reason;
			case "invalid": return [...new Set(failure!.codes.map(code => t(isTextKey(code) ? code : "invalidInput")))].join(" ") || t("invalidInput");
			case "unauthorized": return t(unauthorized);
			default: return t("unexpectedError");
		}
	}
}
