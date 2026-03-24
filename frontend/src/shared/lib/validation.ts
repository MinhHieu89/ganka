type TFunction = (key: string, options?: Record<string, unknown>) => string

export function createValidationMessages(t: TFunction) {
  return {
    required: t("validation.required"),
    invalidEmail: t("validation.invalidEmail"),
    mustBePositive: t("validation.mustBePositive"),
    mustBeInteger: t("validation.mustBeInteger"),
    mustBeNonNegative: t("validation.mustBeNonNegative"),
    minValue: (min: number) => t("validation.minValue", { min }),
    maxValue: (max: number) => t("validation.maxValue", { max }),
    between: (min: number, max: number) => t("validation.between", { min, max }),
    exactDigits: (count: number) => t("validation.exactDigits", { count }),
    minItems: (min: number) => t("validation.minItems", { min }),
    selectRequired: t("validation.selectRequired"),
    minLength: (min: number) => t("validation.minLength", { min }),
    maxLength: (max: number) => t("validation.maxLength", { max }),
    cannotBeZero: t("validation.cannotBeZero"),
    reasonRequired: t("validation.reasonRequired"),
    percentMax100: t("validation.percentMax100"),
  }
}

export type ValidationMessages = ReturnType<typeof createValidationMessages>
