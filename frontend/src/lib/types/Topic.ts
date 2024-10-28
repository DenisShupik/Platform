import type { Category } from "./Category";

export interface Topic {
    postId: number;
    categoryId: Pick<Category, 'categoryId'>;
    title: string;
    created: string;
    createdBy: Date;
}
