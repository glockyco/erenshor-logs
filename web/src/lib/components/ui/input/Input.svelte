<script lang="ts">
  import { cn } from "$lib/utils";

  interface Props {
    type?: "text" | "url" | "number" | "email" | "password";
    value?: string | number;
    placeholder?: string;
    disabled?: boolean;
    readonly?: boolean;
    error?: boolean;
    monospace?: boolean;
    class?: string;
    id?: string;
    min?: number;
    max?: number;
    step?: number;
    "aria-describedby"?: string;
    oninput?: (event: Event) => void;
    onchange?: (event: Event) => void;
    onfocus?: (event: FocusEvent) => void;
    onblur?: (event: FocusEvent) => void;
  }

  let {
    type = "text",
    value = $bindable(""),
    placeholder,
    disabled = false,
    readonly = false,
    error = false,
    monospace = false,
    class: className,
    id,
    min,
    max,
    step,
    "aria-describedby": ariaDescribedby,
    oninput,
    onchange,
    onfocus,
    onblur,
  }: Props = $props();
</script>

<input
  {type}
  bind:value
  {placeholder}
  {disabled}
  {readonly}
  {id}
  {min}
  {max}
  {step}
  aria-invalid={error}
  aria-describedby={ariaDescribedby}
  class={cn(
    "w-full rounded-lg border bg-stone-800 px-4 py-2.5 text-stone-100 transition-colors",
    "placeholder:text-stone-500",
    "focus:outline-none focus:ring-2",
    error
      ? "border-rose-500 focus:border-rose-500 focus:ring-rose-500/50"
      : "border-stone-700 focus:border-amber-500 focus:ring-amber-500/50",
    "disabled:cursor-not-allowed disabled:opacity-50",
    "readonly:bg-stone-900 readonly:cursor-default",
    monospace && "font-mono",
    className
  )}
  {oninput}
  {onchange}
  {onfocus}
  {onblur}
/>
