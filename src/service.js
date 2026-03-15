import axios from 'axios';

// הגדרת הכתובת הבסיסיתם
axios.defaults.baseURL = "http://localhost:5024";

// הוספת interceptor לתפיסת שגיאות
axios.interceptors.response.use(
    response => response,
    error => {
        console.error("שגיאת API:", error);
        return Promise.reject(error);
    }
);

export default {
    // שליפת משימות
    getTasks: async () => {
        const result = await axios.get(`/items`);    
        return result.data;
    },

    // הוספת משימה חדשה
    addTask: async (name) => {
        // שולח אובייקט עם שם המשימה וסטטוס "לא בוצע"
        const result = await axios.post(`/items`, { name: name, isComplete: false });
        return result.data;
    },

   // הוספת name כאן כדי שלא יימחק בעדכון
    setCompleted: async (id, isComplete, name) => {
        const result = await axios.put(`/items/${id}`, { 
            id: id, 
            name: name, 
            isComplete: isComplete 
        });
        return result.data;
    },

    // מחיקת משימה
    deleteTask: async (id) => {
        const result = await axios.delete(`/items/${id}`);
        return result.data;
    }
};