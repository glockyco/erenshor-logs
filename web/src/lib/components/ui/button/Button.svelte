<script lang="ts">
  import { tv, type VariantProps } from "tailwind-variants";
  import type { Snippet } from "svelte";

  const button = tv({
    base: "inline-flex items-center justify-center gap-2 rounded-lg font-semibold transition-all active:scale-95 disabled:cursor-not-allowed disabled:opacity-50",
    variants: {
      variant: {
        default: "bg-amber-500 text-stone-900 hover:bg-amber-400",
        secondary: "bg-stone-700 text-stone-100 hover:bg-stone-600",
        ghost: "bg-transparent text-stone-200 hover:bg-stone-800",
        destructive: "bg-rose-600 text-stone-100 hover:bg-rose-500",
      },
      size: {
        default: "px-6 py-3 text-base",
        sm: "px-4 py-2 text-sm",
        lg: "px-8 py-4 text-lg",
        icon: "p-2",
      },
    },
    defaultVariants: {
      variant: "default",
      size: "default",
    },
  });

  type ButtonVariants = VariantProps<typeof button>;

  interface Props extends ButtonVariants {
    type?: "button" | "submit" | "reset";
    disabled?: boolean;
    class?: string;
    onclick?: (event: MouseEvent) => void;
    children?: Snippet;
  }

  let {
    type = "button",
    variant,
    size,
    disabled = false,
    class: className,
    onclick,
    children,
  }: Props = $props();
</script>

<button {type} {disabled} class={button({ variant, size, class: className })} {onclick}>
  {@render children?.()}
</button>
