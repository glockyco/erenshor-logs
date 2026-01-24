<script lang="ts">
  import { cn } from "$lib/utils";

  interface Props {
    checked?: boolean;
    disabled?: boolean;
    label?: string;
    helpText?: string;
    class?: string;
    id?: string;
    onchange?: (checked: boolean) => void;
  }

  let {
    checked = $bindable(false),
    disabled = false,
    label,
    helpText,
    class: className,
    id = `checkbox-${Math.random().toString(36).slice(2, 9)}`,
    onchange,
  }: Props = $props();

  function handleChange(event: Event) {
    const target = event.target as HTMLInputElement;
    checked = target.checked;
    if (onchange) {
      onchange(target.checked);
    }
  }
</script>

<div class={cn("flex items-start gap-3", className)}>
  <input
    {id}
    type="checkbox"
    {checked}
    {disabled}
    onchange={handleChange}
    aria-checked={checked}
    aria-describedby={helpText ? `${id}-help` : undefined}
    class={cn(
      "mt-0.5 h-5 w-5 cursor-pointer rounded border-2 transition-colors",
      "focus:outline-none focus:ring-2 focus:ring-offset-2",
      checked
        ? "border-amber-500 bg-amber-500 focus:ring-amber-500/50"
        : "border-stone-600 bg-stone-800 hover:border-stone-500 focus:ring-amber-500/50",
      disabled && "cursor-not-allowed opacity-50"
    )}
  />

  {#if label || helpText}
    <div class="flex flex-1 flex-col gap-1">
      {#if label}
        <label
          for={id}
          class={cn(
            "text-sm font-medium text-stone-200",
            disabled ? "cursor-not-allowed" : "cursor-pointer"
          )}
        >
          {label}
        </label>
      {/if}
      {#if helpText}
        <span id="{id}-help" class="text-sm text-stone-400">
          {helpText}
        </span>
      {/if}
    </div>
  {/if}
</div>
