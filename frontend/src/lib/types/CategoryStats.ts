import type { Category } from "./Category";

export interface CategoryStats extends Pick<Category, "categoryId"> {
    topicCount: number;
}